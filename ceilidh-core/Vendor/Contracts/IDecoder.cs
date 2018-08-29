using System;
using System.Collections.Generic;
using System.IO;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.Contracts
{
    [Contract]
    public interface IDecoder
    {
        bool TryDecode(Stream source, out AudioData audioData);
    }

    public abstract class AudioData : IDisposable
    {
        public abstract IReadOnlyDictionary<string, string> Metadata { get; }
        public abstract int StreamCount { get; }
        public abstract int SelectedStream { get; }
        public abstract bool TrySelectStream(int streamIndex);
        public abstract AudioStream GetAudioStream();

        public abstract void Dispose();
    }

    public abstract class AudioStream : Stream
    {
        public abstract AudioFormat Format { get; }
        public abstract long TotalSamples { get; }
        public TimeSpan Duration => TimeSpan.FromSeconds(TotalSamples / (double)Format.SampleRate);

        public sealed override bool CanWrite => false;
        public sealed override bool CanRead => true;
        public override long Length => TotalSamples * Format.BytesPerSample * Format.Channels;
        public sealed override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public sealed override void Flush() => throw new NotSupportedException();

        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            if (origin != SeekOrigin.Begin) throw new ArgumentOutOfRangeException(nameof(origin));

            Seek(TimeSpan.FromSeconds(offset / Format.BytesPerSample / Format.Channels * Format.SampleRate));

            return offset;
        }

        public sealed override void SetLength(long value) => throw new NotSupportedException();

        public abstract void Seek(TimeSpan offset);
    }

    public readonly struct AudioFormat
    {
        public readonly int SampleRate, Channels, BytesPerSample;
        public readonly AudioDataFormat DataFormat;

        public AudioFormat(int sampleRate, int channels, int bytesPerSample, AudioDataFormat dataFormat)
        {
            SampleRate = sampleRate;
            Channels = channels;
            BytesPerSample = bytesPerSample;
            DataFormat = dataFormat;
        }

        public void Deconstruct(out int sampleRate, out int channels, out int bytesPerSample, out AudioDataFormat dataFormat)
        {
            sampleRate = SampleRate;
            channels = Channels;
            bytesPerSample = BytesPerSample;
            dataFormat = DataFormat;
        }
    }

    public enum AudioDataFormat
    {
        U8,
        S8,
        S16,
        S24,
        S32,
        F32,
        F64
    }
}