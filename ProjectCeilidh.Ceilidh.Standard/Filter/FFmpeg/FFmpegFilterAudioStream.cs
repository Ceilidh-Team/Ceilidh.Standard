using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using ProjectCeilidh.Ceilidh.Standard.Audio;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg;
using static FFmpeg.AutoGen.ffmpeg;

namespace ProjectCeilidh.Ceilidh.Standard.Filter.FFmpeg
{
    public unsafe class FFmpegFilterAudioStream : AudioStream
    {
        private static readonly int EAgain = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 35 : 11;

        private static readonly AVFilter* ABufferFilter = avfilter_get_by_name("abuffer");
        private static readonly AVFilter* ABufferSinkFilter = avfilter_get_by_name("abuffersink");

        public override bool CanSeek => _baseAudioStream.CanSeek;
        public override long Position
        {
            get => _baseAudioStream.Position / _baseAudioStream.Format.Channels / _baseAudioStream.Format.DataFormat.BytesPerSample * Format.Channels * Format.DataFormat.BytesPerSample;
            set => _baseAudioStream.Position = value / Format.Channels / Format.DataFormat.BytesPerSample *
                                               _baseAudioStream.Format.Channels *
                                               _baseAudioStream.Format.DataFormat.BytesPerSample;
        }
        public override AudioFormat Format { get; }
        public override long TotalSamples => _baseAudioStream.TotalSamples;

        private readonly AudioStream _baseAudioStream;
        private readonly AVFilterContext* _abufferContext;
        private readonly AVFilterContext* _abuffersinkContext;
        private AVFilterGraph* _filterGraph;
        private AVFrame* _frame;
        private long _samplePosition;

        private int _extraPtr;
        private byte[] _extraData;

        public FFmpegFilterAudioStream(AudioStream baseAudioStream, AudioFormat outputFormat, params FilterConfiguration[] filterOptions) : base(baseAudioStream.ParentData)
        {
            lock (FFmpegDecoder.SyncObject)
            {
                Format = outputFormat;

                _baseAudioStream = baseAudioStream;
                var filters = new AVFilter*[filterOptions.Length];
                var filterContexts = new AVFilterContext*[filterOptions.Length];

                _filterGraph = avfilter_graph_alloc();

                _abufferContext = avfilter_graph_alloc_filter(_filterGraph, ABufferFilter, "src");

                av_opt_set(_abufferContext, "channel_layout", $"{baseAudioStream.Format.Channels}c",
                    AV_OPT_SEARCH_CHILDREN);
                av_opt_set(_abufferContext, "sample_fmt",
                    av_get_sample_fmt_name(baseAudioStream.Format.DataFormat.GetSampleFormat()),
                    AV_OPT_SEARCH_CHILDREN);
                av_opt_set_q(_abufferContext, "time_base",
                    new AVRational {num = 1, den = baseAudioStream.Format.SampleRate}, AV_OPT_SEARCH_CHILDREN);
                av_opt_set_int(_abufferContext, "sample_rate", baseAudioStream.Format.SampleRate,
                    AV_OPT_SEARCH_CHILDREN);

                avfilter_init_str(_abufferContext, null); // TODO: error code

                for (var i = 0; i < filterOptions.Length; i++)
                {
                    filters[i] = avfilter_get_by_name(filterOptions[i].Name);
                    filterContexts[i] = avfilter_graph_alloc_filter(_filterGraph, filters[i], null);

                    foreach (var (key, value) in filterOptions[i].Options)
                        av_opt_set(filterContexts[i], key, value, AV_OPT_SEARCH_CHILDREN);

                    avfilter_init_str(filterContexts[i], null); // TODO: error code
                }

                _abuffersinkContext = avfilter_graph_alloc_filter(_filterGraph, ABufferSinkFilter, "sink");

                avfilter_init_str(_abuffersinkContext, null); // TODO: error code

                for (var i = 0; i <= filterOptions.Length; i++)
                {
                    if (i == 0)
                        avfilter_link(_abufferContext, 0, filterContexts[i], 0); // TODO: error code
                    else if (i == filterOptions.Length)
                        avfilter_link(filterContexts[i - 1], 0, _abuffersinkContext, 0); // TODO: error code
                    else
                        avfilter_link(filterContexts[i - 1], 0, filterContexts[i], 0); // TODO: error code
                }

                avfilter_graph_config(_filterGraph, null); // TODO: error code

                _frame = av_frame_alloc();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_extraData != null)
            {
                var readLen = Math.Min(count, _extraData.Length - _extraPtr);

                fixed (byte* bufPtr = &buffer[offset])
                fixed (byte* oldPtr = &_extraData[_extraPtr])
                    Buffer.MemoryCopy(oldPtr, bufPtr, count, readLen);

                _extraPtr += readLen;
                if (_extraPtr >= _extraData.Length)
                    _extraData = null;

                return readLen;
            }

            lock (FFmpegDecoder.SyncObject)
            {
                int recieveError;
                while ((recieveError = av_buffersink_get_frame(_abuffersinkContext, _frame)) == -EAgain)
                {
                    var buf = new byte[count];

                    var readLen = _baseAudioStream.Read(buf, 0, buf.Length);

                    if (readLen == 0) return 0;

                    _frame->sample_rate = _baseAudioStream.Format.SampleRate;
                    _frame->format = (int)_baseAudioStream.Format.DataFormat.GetSampleFormat();
                    _frame->channel_layout =
                        unchecked((ulong) av_get_default_channel_layout(_baseAudioStream.Format.Channels));
                    _frame->pts = _samplePosition;
                    _frame->nb_samples = readLen / _baseAudioStream.Format.Channels /
                                         _baseAudioStream.Format.DataFormat.BytesPerSample;
                    _samplePosition += _frame->nb_samples;

                    av_frame_get_buffer(_frame, 0);

                    fixed (byte* ptr = buf)
                        Buffer.MemoryCopy(ptr, _frame->extended_data[0], readLen, readLen);

                    av_buffersrc_add_frame(_abufferContext, _frame); // TODO: handle error
                }

                switch (recieveError)
                {
                    case var eof when eof == AVERROR_EOF:
                        return 0;
                    case -EINVAL:
                        throw new ObjectDisposedException(nameof(AVCodec));
                    case var code when code < 0:
                        throw new Exception(); // TODO
                    default:
                        var bps = Format.DataFormat.BytesPerSample;

                        try
                        {
                            fixed (byte* bufPtr = &buffer[offset])
                            {
                                var extLen = _frame->nb_samples * bps * _frame->channels;

                                var readLen = Math.Min(extLen, count);
                                Buffer.MemoryCopy(_frame->extended_data[0], bufPtr, count, readLen);

                                if (readLen != count || readLen == extLen) return readLen;

                                _extraPtr = 0;
                                fixed (byte* extraPtr = _extraData = new byte[extLen - readLen])
                                    Buffer.MemoryCopy(_frame->extended_data[0] + readLen, extraPtr, extLen - readLen,
                                        extLen - readLen);

                                return readLen;
                            }
                        }
                        finally
                        {
                            av_frame_unref(_frame);
                        }
                }
            }
        }
        
        public override void Seek(TimeSpan timestamp) => _baseAudioStream.Seek(timestamp);

        protected override void Dispose(bool disposing)
        {
            lock (FFmpegDecoder.SyncObject)
            {
                fixed (AVFilterGraph** graph = &_filterGraph)
                    avfilter_graph_free(graph);
                fixed (AVFrame** frame = &_frame)
                    av_frame_free(frame);
            }

            base.Dispose(disposing);
        }
    }
}
