using System;
using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Audio
{
    public class BufferedAudioStream : AudioStream
    {
        public override bool CanSeek => _baseStream.CanSeek;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        public override AudioFormat Format => _baseStream.Format;

        public override long TotalSamples => _baseStream.TotalSamples;

        private readonly AudioStream _baseStream;
        private readonly BufferedStream _bufferedStream;

        public BufferedAudioStream(AudioStream baseStream) : base(baseStream.ParentData)
        {
            _baseStream = baseStream;
            _bufferedStream = new BufferedStream(baseStream);
        }

        public BufferedAudioStream(AudioStream baseStream, int bufferSize) : base(baseStream.ParentData)
        {
            _baseStream = baseStream;
            _bufferedStream = new BufferedStream(baseStream, bufferSize);
        }

        public override int Read(byte[] buffer, int offset, int count) => _bufferedStream.Read(buffer, offset, count);

        public override void Seek(TimeSpan timestamp)
        {
            _baseStream.Seek(timestamp);
            _baseStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseStream.Dispose();
                _bufferedStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
