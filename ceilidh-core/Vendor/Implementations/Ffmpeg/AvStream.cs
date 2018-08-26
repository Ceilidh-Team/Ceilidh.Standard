using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AvStream
    {
        public AvDictionary Metadata => _metadata == null ? null : new AvDictionary(_metadata, false);
        public TimeSpan StartTime
        {
            get => TimeSpan.FromSeconds((double) _timeBase * _startTime);
            set => _startTime = (long) (value.TotalSeconds / (double) _timeBase);
        }
        public TimeSpan Duration => TimeSpan.FromSeconds((double)_timeBase * _duration);

#pragma warning disable 169
#pragma warning disable 649
            
        public readonly int Index;
        public readonly int Id;
        [Obsolete]
        public readonly AvCodecContext* Codec;
        private readonly void* _privateData;
        private readonly AvRational _timeBase;
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

    internal unsafe class AvStreamAudioData : AudioStream
    {
        public override bool CanSeek { get; }
        public override long Length { get; }
        public override long Position { get; set; }
        public override AudioFormat Format { get; }

        private int _extraPtr = 0;
        private byte[] _extraData;

        private readonly AvFormatContext _formatContext;
        private readonly AvStream* _stream;
        private readonly AvCodec* _codec;
        private readonly AvCodecContext* _codecContext;
        private readonly AvFrame* _frame;
        
        public AvStreamAudioData(AvFormatContext formatContext, int streamIdx)
        {
            _formatContext = formatContext;

            fixed (AvStream* stream = &formatContext.Streams[streamIdx].Stream)
                _stream = stream;

            _codec = avcodec_find_decoder(_stream->CodecPar->CodecId);
            _codecContext = avcodec_alloc_context3(_codec);
            var code = avcodec_parameters_to_context(_codecContext, _stream->CodecPar);

            var dict = new AvDictionary(new Dictionary<string, string>
            {
                ["refcounted_frames"] = "1"
            });

            fixed (AvDictionaryStruct* ptr = dict)
                if (avcodec_open2(_codecContext, _codec, &ptr) != AvError.Ok)
                    throw new Exception("");

            _frame = av_frame_alloc();
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
                        var code = av_read_frame(format, ref packet); // TODO: EOF
                    }

                    if (packet.StreamIndex == _stream->Index)
                    {
                        switch (avcodec_send_packet(_codecContext, ref packet))
                        {
                            case AvError.EAgain:
                                throw new Exception("Recieved more data before being able to send - this should not happen");
                            case var err when err < 0:
                                throw new Exception($"FFmpeg error code: {err}");
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
                case var err when err < 0: // Generic error
                    throw new Exception($"FFmpeg error code: {err}");
                default:
                    var backing = av_frame_clone(_frame);

                    if (backing->Format.IsPlanar()) // Planar = non-interleaved, so we have to adjust it first
                    {
                        var bps = backing->Format.BytesPerSample();

                        var tmpBuf = new byte[backing->LineSize[0] * _codecContext->Channels];
                        fixed (byte* tmpPtr = tmpBuf)
                        {
                            for (var i = 0; i < _codecContext->Channels; i++)
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

                                
                                for (var j = 0; j < backing->LineSize[0]; j += bps)
                                    *(ulong*) (tmpPtr + i * bps + j * bps * _codecContext->Channels) |= mask & *(ulong*)(backing->ExtendedData[i] + j);
                            }

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
                            var readLen = Math.Min(backing->LineSize[0], buffer.Length);
                            Buffer.MemoryCopy(backing->ExtendedData[0], bufPtr, buffer.Length, readLen);

                            if (readLen == buffer.Length && readLen != backing->LineSize[0])
                                fixed (byte* extraPtr = _extraData = new byte[backing->LineSize[0] - readLen])
                                    Buffer.MemoryCopy(backing->ExtendedData[0] + readLen, extraPtr, backing->LineSize[0] - readLen, backing->LineSize[0] - readLen);

                            return readLen;
                        }
                    }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
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
        private static extern AvFrame* av_frame_clone(AvFrame* frame);

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