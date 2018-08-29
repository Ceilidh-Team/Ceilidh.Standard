using System;
using System.Collections.Generic;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal unsafe class FfmpegAudioData : AudioData
    {
        public override IReadOnlyDictionary<string, string> Metadata =>
            _selectedStream < 0 ? null : _streams[_selectedStream]->Metadata ?? _formatContext.GetFileMetadata();
        public override int StreamCount => _streams.Length;
        public override int SelectedStream => _selectedStream;


        private int _selectedStream = -1;
        private readonly AvIoContext _ioContext;
        private readonly AvFormatContext _formatContext;
        private readonly AvStream*[] _streams;

        public FfmpegAudioData(AvIoContext io, AvFormatContext format)
        {
            _ioContext = io;
            _formatContext = format;

            var tmp = new AvStream*[format.Streams.Length];
            var i = 0;
            foreach (ref readonly var formatStream in format.Streams)
            {
                if (formatStream.Stream.CodecPar->CodecType == AvMediaType.Audio)
                    fixed (AvStream* streamPtr = &formatStream.Stream)
                        tmp[i++] = streamPtr;
            }

            _streams = new AvStream*[i];
            Array.Copy(tmp, _streams, i);
        }

        public override bool TrySelectStream(int streamIndex)
        {
            if (streamIndex < 0 || streamIndex >= _streams.Length)
                return false;

            _selectedStream = streamIndex;
            return true;
        }

        public override AudioStream GetAudioStream() => new AvStreamAudioStream(_formatContext, _streams[_selectedStream]);

        public override void Dispose()
        {
            _formatContext.Dispose();
        }
    }
}