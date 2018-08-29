using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ceilidh.Core.Util;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AvStream
    {
        public AvDictionary Metadata => _metadata == null ? null : new AvDictionary(_metadata, false);
        public TimeSpan StartTime
        {
            get => TimeSpan.FromSeconds((double) TimeBase * _startTime);
            set => _startTime = (long) (value.TotalSeconds / (double) TimeBase);
        }
        public TimeSpan Duration => TimeSpan.FromSeconds((double)TimeBase * _duration);

#pragma warning disable 169
#pragma warning disable 649
            
        public readonly int Index;
        public readonly int Id;
        [Obsolete]
        public AvCodecContext* Codec;
        private readonly void* _privateData;
        public readonly AvRational TimeBase;
        private long _startTime;
        private readonly long _duration;
        public readonly long FrameCount;
        public readonly AvDisposition Disposition;
        public AvDiscard Discard;
        public readonly AvRational SampleAspectRatio;
        private readonly AvDictionaryStruct* _metadata;
        public readonly AvRational AverageFrameRate;
        public readonly AvPacket AttachedPic;
        public readonly void* SideData;
        public readonly int SideDataCount;
        public readonly int EventFlags;
        public readonly AvRational FrameRate;
        public readonly byte* RecommendedEncoderConfiguration;
        public readonly AvCodecParameters* CodecPar;

#pragma warning restore 169
#pragma warning restore 649
    }

    internal unsafe class AvStreamAudioStream : AudioStream
    {
        public override bool CanSeek => _formatContext.CanSeek;

        public override long Position { get; set; }
        public override AudioFormat Format => new AudioFormat(_codecContext->SampleRate, _codecContext->Channels, _codecContext->SampleFormat.BytesPerSample(), AudioDataFormat.S8); // TODO: Real data format
        public override long TotalSamples => (long) (_stream->Duration.TotalSeconds * _codecContext->SampleRate);

        private int _extraPtr;
        private byte[] _extraData;

        private readonly AvFormatContext _formatContext;
        private readonly AvStream* _stream;
        private AvCodecContext* _codecContext;
        private AvFrame* _frame;
        
        public AvStreamAudioStream(AvFormatContext formatContext, AvStream* stream)
        {
            _formatContext = formatContext;

            _stream = stream;

            var codec = avcodec_find_decoder(_stream->CodecPar->CodecId);

            if (codec == null)
                throw new LocalizedException(new InvalidDataException("ffmpeg.error.unsupported_codec"));

            _codecContext = avcodec_alloc_context3(codec);

            if (_codecContext == null)
                throw new LocalizedException(new Exception("ffmpeg.error.unknown"), nameof(avcodec_alloc_context3));

            if(avcodec_parameters_to_context(_codecContext, _stream->CodecPar) != AvError.Ok)
                throw new LocalizedException(new Exception("ffmpeg.error.unknown"), nameof(avcodec_parameters_to_context));

            fixed (AvDictionaryStruct* ptr = new AvDictionary(new Dictionary<string, string>
            {
                ["refcounted_frames"] = "1"
            }))
                if (avcodec_open2(_codecContext, codec, &ptr) != AvError.Ok)
                    throw new LocalizedException(new Exception("ffmpeg.error.unknown"), nameof(avcodec_open2));

            _frame = av_frame_alloc();

            if (_frame == null)
                throw new LocalizedException(new Exception("ffmpeg.error.unknown"), nameof(av_frame_alloc));
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
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

            AvError recieveError;
            while ((recieveError = avcodec_receive_frame(_codecContext, _frame)) == AvError.EAgain)
            {
                AvPacket packet = default;
                try
                {
                    av_init_packet(ref packet);

                    packet.Data = null;
                    packet.Size = 0;

                    fixed (AvFormatContextStruct* format = _formatContext)
                    {
                        switch (av_read_frame(format, ref packet))
                        {
                            case AvError.Eof:
                                return 0;
                            case var code when code < 0:
                                throw new LocalizedException(new Exception("ffmpeg.error.averror"), nameof(av_read_frame), code);
                        }
                    }

                    if (packet.StreamIndex == _stream->Index)
                    {
                        switch (avcodec_send_packet(_codecContext, ref packet))
                        {
                            case AvError.EAgain:
                                throw new IOException();
                            case var code when code < 0:
                                throw new LocalizedException(new Exception("ffmpeg.error.averror"), nameof(avcodec_send_packet), code);
                        }
                    }
                }
                finally
                {
                    av_packet_unref(ref packet);
                }
            }

            switch (recieveError)
            {
                case AvError.Eof:
                    return 0;
                case AvError.EInval:
                    throw new ObjectDisposedException(nameof(AvCodec));
                case var code when code < 0:
                    throw new LocalizedException(new Exception("ffmpeg.error.averror"), nameof(avcodec_receive_frame), code);
                default:
                    // Console.WriteLine(_codecContext->FrameNumber);

                    var time = TimeSpan.FromSeconds(_frame->PresentationTimeStamp * (double) _codecContext->TimeBase);
                    Console.WriteLine(time);
                    
                    var bps = _frame->Format.BytesPerSample();

                    if (_frame->Format.IsPlanar() && _codecContext->Channels > 1) // Planar = non-interleaved, so we have to adjust it first. Only one channel is equivalent to interleaved
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

                        var tmpBuf = new byte[_frame->SampleCount * bps * _codecContext->Channels];
                        fixed (byte* tmpPtr = tmpBuf)
                        {
                            for (var i = 0; i < _codecContext->Channels; i++)
                                for(var j = 0; j < _frame->SampleCount * bps; j += bps)
                                    *(ulong*) (tmpPtr + i * bps + j * _codecContext->Channels) |= mask & *(ulong*)(_frame->ExtendedData[i] + j);

                            fixed (byte* bufPtr = buffer)
                            {
                                var readLen = Math.Min(tmpBuf.Length, buffer.Length);
                                Buffer.MemoryCopy(tmpPtr, bufPtr, buffer.Length, readLen);

                                if (readLen == buffer.Length && readLen != tmpBuf.Length)
                                    fixed (byte* extraPtr = _extraData = new byte[tmpBuf.Length - readLen])
                                        Buffer.MemoryCopy(tmpPtr + readLen, extraPtr, tmpBuf.Length - readLen, tmpBuf.Length - readLen);

                                return readLen;
                            }
                        }
                    }
                    else // Interleaved data can be emitted directly
                    {
                        fixed (byte* bufPtr = buffer)
                        {
                            var extLen = _frame->SampleCount * bps;

                            var readLen = Math.Min(extLen, buffer.Length);
                            Buffer.MemoryCopy(_frame->ExtendedData[0], bufPtr, buffer.Length, readLen);

                            if (readLen == buffer.Length && readLen != extLen)
                                fixed (byte* extraPtr = _extraData = new byte[extLen - readLen])
                                    Buffer.MemoryCopy(_frame->ExtendedData[0] + readLen, extraPtr, extLen - readLen, extLen - readLen);

                            return readLen;
                        }
                    }
            }
        }

        public override void Seek(TimeSpan offset)
        {
            fixed (AvFormatContextStruct* fCtx = _formatContext)
            {
                var ts = (long) (offset.TotalSeconds * (double) _stream->TimeBase);

                if(avformat_seek_file(fCtx, _stream->Index, 0, ts, ts, AvSeekFlag.Any) < 0)
                    throw new LocalizedException(new Exception("ffmpeg.error.unknown"), nameof(avformat_seek_file));
                avcodec_flush_buffers(_codecContext);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_frame != null)
                av_frame_free(ref _frame);
            if (_codecContext != null)
                avcodec_free_context(ref _codecContext);

            _stream->Codec = null;
        }

        #region Native

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvError av_find_best_stream(AvFormatContextStruct* ic, AvMediaType type, int wantedNb,
            int relatedStream, ref AvCodec* decoder, int flags);

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvError avformat_seek_file(AvFormatContextStruct* s, int streamIndex, long minTs, long ts,
            long maxTs, AvSeekFlag flags);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern AvFrame* av_frame_alloc();

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern void av_frame_free(ref AvFrame* frame);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern void avcodec_free_context(ref AvCodecContext* context);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern AvError avcodec_send_packet(AvCodecContext* avctx, ref AvPacket avpkt);

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvError av_read_frame(AvFormatContextStruct* s, ref AvPacket pkt);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern void avcodec_flush_buffers(AvCodecContext* avctx);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern void av_init_packet(ref AvPacket packet);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern void av_packet_unref(ref AvPacket packet);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern AvError avcodec_receive_frame(AvCodecContext* context, AvFrame* frame);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern AvError avcodec_parameters_to_context(AvCodecContext* context, AvCodecParameters* codecpar);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern AvError avcodec_open2(AvCodecContext* context, AvCodec* codec, AvDictionaryStruct** options);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern AvCodecContext* avcodec_alloc_context3(AvCodec* codec);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern AvCodec* avcodec_find_decoder(int codecId);

        #endregion
    }
}