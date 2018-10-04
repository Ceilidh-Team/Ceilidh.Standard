using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg
{
    internal unsafe class FFmpegAudioStream : AudioStream
    {
        private static readonly int EAgain = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 35 : 11;

        public override bool CanSeek => _formatContext->pb->seek.Pointer != IntPtr.Zero;
        public override long Position { get; set; }
        public override AudioFormat Format => new AudioFormat(_codecContext->sample_rate, _codecContext->channels,
            new AudioDataFormat(GetNumberFormat(_codecContext->sample_fmt), !BitConverter.IsLittleEndian,
                av_get_bytes_per_sample(_codecContext->sample_fmt)));

        public override long TotalSamples => _stream->duration * _stream->time_base.num / _stream->time_base.den *
                                             _codecContext->sample_rate;
            
        private int _extraPtr;
        private byte[] _extraData;

        private AVCodecContext* _codecContext;
        private AVFrame* _avFrame;
        private readonly AVFormatContext* _formatContext;
        private readonly AVStream* _stream;

        public FFmpegAudioStream(AudioData audioData, AVFormatContext* formatContext, AVStream* stream) : base(audioData)
        {
            _formatContext = formatContext;
            _stream = stream;

            var codec = avcodec_find_decoder(_stream->codecpar->codec_id);

            _codecContext = avcodec_alloc_context3(codec);

            if (avcodec_parameters_to_context(_codecContext, _stream->codecpar) != 0)
                throw new Exception(); // TODO

            AVDictionary* dict;
            av_dict_set_int(&dict, "refcounted_frames", 1, 0);

            if (avcodec_open2(_codecContext, codec, &dict) != 0)
                throw new Exception();

            _avFrame = av_frame_alloc();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_extraData != null)
            {
                var readLen = Math.Min(buffer.Length, _extraData.Length - _extraPtr);

                fixed (byte* bufPtr = buffer)
                fixed (byte* oldPtr = &_extraData[_extraPtr])
                    Buffer.MemoryCopy(oldPtr, bufPtr, buffer.Length, readLen);

                _extraPtr += readLen;
                if (_extraPtr >= _extraData.Length)
                    _extraData = null;

                return readLen;
            }

            int recieveError;
            while ((recieveError = avcodec_receive_frame(_codecContext, _avFrame)) == -EAgain)
            {
                AVPacket packet = default;
                try
                {
                    av_init_packet(&packet);

                    packet.data = null;
                    packet.size = 0;

                    switch (av_read_frame(_formatContext, &packet))
                    {
                        case var eof when eof == AVERROR_EOF:
                            return 0;
                        case var code when code < 0:
                            throw new Exception(""); // TODO
                    }

                    if (packet.stream_index == _stream->index)
                    {
                        switch (avcodec_send_packet(_codecContext, &packet))
                        {
                            case var again when again == -EAgain:
                                throw new IOException();
                            case var code when code < 0:
                                throw new Exception(); // TODO
                        }
                    }
                }
                finally 
                {
                    av_packet_unref(&packet);
                }
            }

            switch (recieveError)
            {
                case var eof when eof == AVERROR_EOF:
                    return 0;
                case -EINVAL:
                    throw new ObjectDisposedException(nameof(AVCodec));
                case var code when code < 0:
                    throw new Exception(); // TODO
                default:
                    var bps = av_get_bytes_per_sample((AVSampleFormat) _avFrame->format);

                    if (av_sample_fmt_is_planar((AVSampleFormat) _avFrame->format) != 0 && _avFrame->channels > 1)
                    {
                        ulong mask;
                        switch (bps)
                        {
                            case 1:
                                mask = 0xFFL;
                                break;
                            case 2:
                                mask = 0xFFFFL;
                                break;
                            case 4:
                                mask = 0xFFFFFFFFL;
                                break;
                            case 8:
                                mask = 0xFFFFFFFFFFFFFFFFL;
                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }

                        var tmpBuf = new byte[_avFrame->nb_samples * bps * _avFrame->channels];
                        fixed (byte* tmpPtr = tmpBuf)
                        {
                            for (var i = 0; i < _avFrame->channels; i++)
                                for (var j = 0; j < _avFrame->nb_samples * bps; j += bps)
                                    *(ulong*)(tmpPtr + i * bps + j * _avFrame->channels) |= mask & *(ulong*)(_avFrame->extended_data[i] + j);

                            fixed (byte* bufPtr = buffer)
                            {
                                var readLen = Math.Min(tmpBuf.Length, buffer.Length);
                                Buffer.MemoryCopy(tmpPtr, bufPtr, buffer.Length, readLen);

                                if (readLen == buffer.Length && readLen != tmpBuf.Length)
                                {
                                    _extraPtr = 0;
                                    fixed (byte* extraPtr = _extraData = new byte[tmpBuf.Length - readLen])
                                        Buffer.MemoryCopy(tmpPtr + readLen, extraPtr, tmpBuf.Length - readLen, tmpBuf.Length - readLen);
                                }

                                return readLen;
                            }
                        }
                    }
                    else
                    {
                        fixed (byte* bufPtr = buffer)
                        {
                            var extLen = _avFrame->nb_samples * bps * _avFrame->channels;

                            var readLen = Math.Min(extLen, buffer.Length);
                            Buffer.MemoryCopy(_avFrame->extended_data[0], bufPtr, buffer.Length, readLen);

                            if (readLen == buffer.Length && readLen != extLen)
                            {
                                _extraPtr = 0;
                                fixed (byte* extraPtr = _extraData = new byte[extLen - readLen])
                                    Buffer.MemoryCopy(_avFrame->extended_data[0] + readLen, extraPtr, extLen - readLen, extLen - readLen);
                            }

                            return readLen;
                        }
                    }
            }
        }

        public override void Seek(TimeSpan timestamp)
        {
            var ts = (long) (timestamp.TotalSeconds / (_stream->time_base.num / (double)_stream->time_base.den));

            if (avformat_seek_file(_formatContext, _stream->index, 0, ts, ts, AVSEEK_FLAG_ANY) < 0)
                throw new Exception(); // TODO

            avcodec_flush_buffers(_codecContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (_avFrame != null)
                fixed (AVFrame** framePtr = &_avFrame)
                    av_frame_free(framePtr);

            if (_codecContext != null)
                fixed (AVCodecContext** codecPtr = &_codecContext)
                    avcodec_free_context(codecPtr);
        }

        private static NumberFormat GetNumberFormat(AVSampleFormat sampleFormat)
        {
            switch (sampleFormat)
            {
                case AVSampleFormat.AV_SAMPLE_FMT_DBL:
                case AVSampleFormat.AV_SAMPLE_FMT_DBLP:
                case AVSampleFormat.AV_SAMPLE_FMT_FLT:
                case AVSampleFormat.AV_SAMPLE_FMT_FLTP:
                    return NumberFormat.FloatingPoint;
                case AVSampleFormat.AV_SAMPLE_FMT_U8:
                case AVSampleFormat.AV_SAMPLE_FMT_U8P:
                    return NumberFormat.Unsigned;
                default:
                    return NumberFormat.Signed;
            }
        }
    }
}
