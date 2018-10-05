using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg
{
    internal unsafe class FFmpegAudioData : AudioData
    {
        public override IReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                lock (FFmpegDecoder.SyncObject)
                {
                    var metadataDict = _streams[_selectedStream]->metadata;
                    if (metadataDict == null)
                        metadataDict = _formatContext->metadata;

                    var dict = new Dictionary<string, string>();

                    AVDictionaryEntry* entry = null;
                    while ((entry = av_dict_get(metadataDict, "", entry, AV_DICT_IGNORE_SUFFIX)) != null)
                        dict.Add(Marshal.PtrToStringAnsi(new IntPtr(entry->key)),
                            Marshal.PtrToStringAnsi(new IntPtr(entry->value)));

                    return dict;
                }
            }
        }

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

            StreamCount = j;
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

        public override AudioStream GetAudioStream() => new FFmpegAudioStream(this, _formatContext, _streams[SelectedStream]);

        public override void Dispose()
        {
            if (_formatContext != null)
            {
                lock (FFmpegDecoder.SyncObject)
                {
                    if (_formatContext->pb != null && _formatContext->pb->buffer != null)
                        av_freep(&_formatContext->pb->buffer);

                    if (_formatContext->pb != null)
                    {
                        var handle = GCHandle.FromIntPtr(new IntPtr(_formatContext->pb->opaque));

                        ((Stream) handle.Target).Dispose();
                        handle.Free();

                        avio_context_free(&_formatContext->pb);
                    }

                    fixed (AVFormatContext** formatPtr = &_formatContext)
                        avformat_close_input(formatPtr);
                }
            }
        }
    }
}
