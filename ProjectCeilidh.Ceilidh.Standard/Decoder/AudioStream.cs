using System;
using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    public abstract class AudioStream : Stream
    {
        public abstract AudioFormat Format { get; }
        public abstract long TotalSamples { get; }
        public TimeSpan Duration => TimeSpan.FromSeconds(TotalSamples / (double) Format.SampleRate);

        public sealed override bool CanWrite => false;
        public sealed override bool CanRead => true;

        public abstract void Seek(TimeSpan timestamp);

        public override long Length => TotalSamples * Format.DataFormat.BytesPerSample / 8 * Format.Channels;
        public sealed override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public sealed override void Flush() => throw new NotSupportedException();
        public sealed override void SetLength(long value) => throw new NotSupportedException();

        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            if (origin != SeekOrigin.Begin) throw new ArgumentOutOfRangeException(nameof(origin));

            Seek(TimeSpan.FromSeconds(offset / Format.DataFormat.BytesPerSample / Format.Channels *
                                      Format.SampleRate));

            return offset;
        }
    }
}
