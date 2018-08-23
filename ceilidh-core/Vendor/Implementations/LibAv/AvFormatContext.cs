using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ceilidh.Core.Vendor.Implementations.LibAv
{
    internal unsafe class AvFormatContext : IDisposable
    {
        public static Version AvFormatVersion
        {
            get
            {
                var ver = avformat_version();
                return new Version((ver >> 16) & 0xff, (ver >> 8) & 0xff, ver & 0xff);
            }
        }

        public static Version AvCodecVersion
        {
            get
            {
                var ver = avcodec_version();
                return new Version((ver >> 16) & 0xff, (ver >> 8) & 0xff, ver & 0xff);
            }
        }

        private bool _isOpen;
        private AvFormatContextStruct* _basePtr;
        private readonly AvIoContext _context;

        public AvFormatContext(AvIoContext ioContext)
        {
            _context = ioContext;

            _basePtr = avformat_alloc_context();
            _basePtr->pb = _context.BasePointer;
        }

        public AvError OpenInput(string url = "")
        {
            var res = avformat_open_input(ref _basePtr, url, null, null);
            if (res != AvError.Ok)
                Dispose();
            else
                _isOpen = true;

            return res;
        }

        public AvError FindStreamInfo() => avformat_find_stream_info(_basePtr, null);

        public IReadOnlyDictionary<string, string>[] GetStreamMetadata()
        {
            var arr = new IReadOnlyDictionary<string, string>[_basePtr->nb_streams];

            for (var i = 0; i < _basePtr->nb_streams; i++)
                arr[i] = new AvDictionary(_basePtr->streams[i]->metadata);

            return arr;
        }

        public IReadOnlyDictionary<string, string> GetFileMetadata() => new AvDictionary(_basePtr->metadata);

        public void Dispose()
        {
            if (_basePtr != null && _isOpen)
                avformat_close_input(ref _basePtr);
            else if (_basePtr != null)
                avformat_free_context(ref _basePtr);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AvFormatContextStruct
        {
            public void* av_class;
            public void* iformat;
            public void* oformat;
            public void* priv_data;
            public void* pb;
            public int ctx_flags;
            public uint nb_streams;
            public AvStream** streams;
            public fixed byte filename[1024];
            public long start_time;
            public long duration;
            public long bit_rate;
            public uint packet_size;
            public int max_delay;
            public int flags;
            public long probesize;
            public long max_analyze_duration;
            public byte* key;
            public int keylen;
            public uint nb_programs;
            public void** programs;
            public int video_codec_id;
            public int audio_codec_id;
            public int subtitle_codec_id;
            public uint max_index_size;
            public uint max_picture_buffer;
            public uint nb_chapters;
            public void** chapters;
            public void* metadata;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AvStream
        {
            public int index;
            public int id;
            public void* codec;
            public void* priv_data;
            public long time_base;
            public long start_time;
            public long duration;
            public long nb_frames;
            public int disposition;
            public int discard;
            public long sample_aspect_ratio;
            public void* metadata;
        }

        #region Native

#pragma warning disable IDE1006

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvFormatContextStruct* avformat_alloc_context();

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvError avformat_open_input(ref AvFormatContextStruct* context, [MarshalAs(UnmanagedType.LPStr)] string url, void* fmt, void* options);

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern void avformat_close_input(ref AvFormatContextStruct* context);

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern void avformat_free_context(ref AvFormatContextStruct* context);

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern int avformat_version();

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvError avformat_find_stream_info(AvFormatContextStruct* context, void* options);

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern int avcodec_version();

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern AvDictionaryEntry* av_dict_get(void* dict, [MarshalAs(UnmanagedType.LPStr)] string key, AvDictionaryEntry* prevEntry, int flags);

#if WIN32
        [DllImport("avformat-58", EntryPoint = "av_register_all")]
#else
        [DllImport("avformat", EntryPoint = "av_register_all")]
#endif
        public static extern void RegisterAllFormats();


#pragma warning restore IDE1006

        #endregion
    }
}
