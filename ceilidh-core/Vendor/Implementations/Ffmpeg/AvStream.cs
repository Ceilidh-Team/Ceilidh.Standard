using System;
using System.IO;
using System.Runtime.InteropServices;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AvStream
    {
        public AvDictionary Metadata => new AvDictionary(_metadata, false);
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

#pragma warning restore 169
#pragma warning restore 649
    }

    internal unsafe class AvStreamAudioData : AudioStream
    {
        public override bool CanSeek { get; }
        public override long Length { get; }
        public override long Position { get; set; }
        public override AudioFormat Format { get; }

        private readonly AvStream* _stream;

        public AvStreamAudioData(AvStream* stream)
        {
            _stream = stream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}