using System.IO;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.Contracts
{
    [Contract]
    public interface IDecoder
    {
        bool TryDecode(Stream source, out AudioStream audioData);
    }

    public class AudioStream : Stream
    {
        public AudioFormat Format { get; }

        private readonly Stream _baseStream;

        public AudioStream(Stream stream, AudioFormat format)
        {
            _baseStream = stream;
            Format = format;
        }

        #region Stream
        
        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

        public override void Flush() => _baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

        public override void SetLength(long value) => _baseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);
        
        #endregion
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