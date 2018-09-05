using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        private AVFormatContext* _formatContext;
        private readonly AVStream*[] _streams;

        public FFmpegAudioData(AVFormatContext* format)
        {
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

        public override AudioStream GetAudioStream() => new FFmpegAudioStream(_formatContext, _streams[SelectedStream]);

        public override void Dispose()
        {
            if (_formatContext != null)
            {
                if (_formatContext->pb != null && _formatContext->pb->buffer != null)
                    ffmpeg.av_freep(&_formatContext->pb->buffer);

                if (_formatContext->pb != null)
                {
                    var handle = GCHandle.FromIntPtr(new IntPtr(_formatContext->pb->opaque));

                    if (handle.IsAllocated)
                    {
                        ((Stream)handle.Target).Dispose();
                        handle.Free();
                    }

                    ffmpeg.avio_context_free(&_formatContext->pb);
                }

                fixed (AVFormatContext** formatPtr = &_formatContext)
                    ffmpeg.avformat_close_input(formatPtr);
            }
        }
    }
}
