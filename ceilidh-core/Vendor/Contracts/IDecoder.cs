using System;
using System.IO;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.Contracts
{
    [Contract]
    public interface IDecoder
    {
        bool TryDecode(Stream source, out AudioStream audioData);
    }

    public abstract class AudioStream : Stream
    {
        public abstract AudioFormat Format { get; }

        public sealed override bool CanWrite => false;
        public sealed override bool CanRead => true;
        public sealed override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public sealed override void Flush() => throw new NotSupportedException();
    }

    public struct AudioFormat
    {
        public readonly uint SampleRate, Channels, BytesPerChannel;

        public AudioFormat(uint sampleRate, uint channels, uint bytesPerChannel)
        {
            SampleRate = sampleRate;
            Channels = channels;
            BytesPerChannel = bytesPerChannel;
        }

        public void Deconstruct(out uint sampleRate, out uint channels, out uint bytesPerChannel)
        {
            sampleRate = SampleRate;
            channels = Channels;
            bytesPerChannel = BytesPerChannel;
        }
    }
}