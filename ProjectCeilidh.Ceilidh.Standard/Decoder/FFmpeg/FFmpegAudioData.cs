using System;
using System.Collections.Generic;
using System.Text;
using FFmpeg.AutoGen;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg
{
    public unsafe class FFmpegAudioData : AudioData
    {
        public override IReadOnlyDictionary<string, string> Metadata { get; }
        public override int StreamCount { get; }
        public override int SelectedStream => _selectedStream;

        private int _selectedStream = -1;
        private readonly AVIOContext* _ioContext;
        private readonly AVFormatContext* _formatContext;
        private readonly AVStream*[] _streams;

        public FFmpegAudioData(AVIOContext* io, AVFormatContext* format)
        {
            _ioContext = io;
            _formatContext = format;

            var tmp = new AVStream*[format->nb_streams];
            var j = 0;
            for (var i = 0; i < format->nb_streams; i++)
                if (format->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                    tmp[j++] = format->streams[i];

            _streams = new AVStream*[j];
            Array.Copy(tmp, _streams, j);
        }

        public override bool TrySelectStream(int streamIndex)
        {
            if (streamIndex < 0 || streamIndex >= _streams.Length)
                return false;

            _selectedStream = streamIndex;
            return true;
        }

        public override AudioStream GetAudioStream()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
