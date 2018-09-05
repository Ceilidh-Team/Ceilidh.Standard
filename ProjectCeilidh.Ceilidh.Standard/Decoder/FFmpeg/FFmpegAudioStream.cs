using System;
using System.IO;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg
{
    public unsafe class FFmpegAudioStream : AudioStream
    {
        public override bool CanSeek { get; }
        public override long Position { get; set; }
        public override AudioFormat Format { get; }
        public override long TotalSamples { get; }

        private int _extraPtr;
        private byte[] _extraData;

        private AVCodecContext* _codecContext;
        private AVFrame* _avFrame;
        private readonly AVFormatContext* _formatContext;
        private readonly AVStream* _stream;

        public FFmpegAudioStream(AVFormatContext* formatContext, AVStream* stream)
        {
            _formatContext = formatContext;
            _stream = stream;

            var codec = avcodec_find_decoder(_stream->codecpar->codec_id);

            _codecContext = avcodec_alloc_context3(codec);

            if (avcodec_parameters_to_context(_codecContext, _stream->codecpar) != 0)
                throw new Exception(); // TODO

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
                if (_extraPtr == _extraData.Length)
                    _extraData = null;

                return readLen;
            }

            int recieveError;
            while ((recieveError = avcodec_receive_frame(_codecContext, _avFrame)) == -EAGAIN)
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
                            case -EAGAIN:
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

                    if (av_sample_fmt_is_planar((AVSampleFormat) _avFrame->format) != 0)
                    {
                        return 0;
                    }
                    else
                    {
                        fixed (byte* bufPtr = buffer)
                        {
                            var extLen = _avFrame->nb_samples * bps;

                            var readLen = Math.Min(extLen, buffer.Length);
                            Buffer.MemoryCopy(_avFrame->extended_data[0], bufPtr, buffer.Length, readLen);

                            if (readLen == buffer.Length && readLen != extLen)
                                fixed (byte* extraPtr = _extraData = new byte[extLen - readLen])
                                    Buffer.MemoryCopy(_avFrame->extended_data[0] + readLen, extraPtr, extLen - readLen, extLen - readLen);

                            return readLen;
                        }
                    }
            }
        }

        public override void Seek(TimeSpan timestamp)
        {
            var ts = (long) (timestamp.TotalSeconds * _stream->time_base.num / _stream->time_base.den);

            if (avformat_seek_file(_formatContext, _stream->index, 0, ts, ts, AVSEEK_FLAG_ANY) < 0)
                throw new Exception(); // TODO

            avcodec_flush_buffers(_codecContext);
        }
    }
}
