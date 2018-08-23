using System;
using System.Collections.Generic;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal class FfmpegAudioData : AudioData
    {
        public override IReadOnlyDictionary<string, string> Metadata { get; }
        public override int StreamCount { get; }
        public override int SelectedStream { get; }

        private readonly AvIoContext _ioContext;
        private readonly AvFormatContext _formatContext;

        public FfmpegAudioData(AvIoContext io, AvFormatContext format)
        {
            _ioContext = io;
            _formatContext = format;
        }

        public override bool TrySelectStream(int streamIndex)
        {
            return false;
        }

        public override AudioStream GetAudioStream() => throw new NotImplementedException();

        public override void Dispose()
        {
            _formatContext.Dispose();
            _ioContext.Dispose();
        }
    }
}