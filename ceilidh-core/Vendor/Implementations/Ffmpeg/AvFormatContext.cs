using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal readonly unsafe struct AvStreamReference
    {
        public ref readonly AvStream Stream => ref *_stream;

#pragma warning disable 169
#pragma warning disable 649

        private readonly AvStream* _stream;

#pragma warning restore 169
#pragma warning restore 649
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AvFormatContextStruct
    {
        public const int TIME_BASE = 100000;

        public AvIoContext AvIoContext
        {
            get => new AvIoContext(_avIoContext, false);
            set
            {
                fixed (AvIoContextStruct* ptr = value)
                    _avIoContext = ptr;
            }
        }

        public ReadOnlySpan<AvStreamReference> Streams => new ReadOnlySpan<AvStreamReference>(_streams, (int)_streamCount);

        public string Filename
        {
            get
            {
                fixed(byte* ptr = _filename)
                    return Marshal.PtrToStringUTF8(new IntPtr(ptr), 1024);
            }
        }

        public TimeSpan StartTime => new TimeSpan(_startTime * TIME_BASE * 10);
        public TimeSpan Duration => new TimeSpan(_duration * TIME_BASE * 10);

        public TimeSpan MaxAnalyzeDuration
        {
            get => new TimeSpan(_maxAnalyzeDuration * TIME_BASE * 10);
            set => _maxAnalyzeDuration = value.Ticks / TIME_BASE / 10;
        }

        public ReadOnlySpan<byte> Key => new ReadOnlySpan<byte>(_key, _keyLength);

        public AvDictionary Metadata => new AvDictionary(_metadata, false);

#pragma warning disable 169
#pragma warning disable 649

        public readonly void* AvClass;
        public readonly void* InputFormat;
        public readonly void* OutputFormat;
        private readonly void* _privateData;
        private AvIoContextStruct* _avIoContext;
        public readonly int ContextFlags; // TODO: Enum
        private readonly uint _streamCount;
        private readonly AvStream** _streams;
        private fixed byte _filename[1024];
        private readonly long _startTime;
        private readonly long _duration;
        public readonly long BitRate;
        public readonly uint PacketSize;
        public readonly int MaxDelay;
        public int Flags;
        public long ProbeSize;
        private long _maxAnalyzeDuration;
        private readonly byte* _key;
        private readonly int _keyLength;
        private readonly uint _programCount;
        private readonly void** _programs;
        public int VideoCodecId;
        public int AudioCodecId; 
        public int SubtitleCodecId;
        public uint MaxIndexSize;
        public uint MaxPictureBuffer;
        public readonly uint ChapterCount;
        public readonly void** Cahapters;
        private readonly AvDictionaryStruct* _metadata;

#pragma warning restore 169
#pragma warning restore 649
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AvStream
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
        public AvDictionaryStruct* metadata;
    }

    internal unsafe class AvFormatContext : IDisposable
    {
        private bool _isOpen;
        private AvFormatContextStruct* _basePtr;
        private readonly AvIoContext _context;

        public AvFormatContext(AvIoContext ioContext)
        {
            _context = ioContext;

            _basePtr = avformat_alloc_context();
            _basePtr->AvIoContext = ioContext;
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
            var streams = _basePtr->Streams;

            var arr = new IReadOnlyDictionary<string, string>[streams.Length];

            for(var i = 0; i < streams.Length; i++)
                arr[i] = new AvDictionary(streams[i].Stream.metadata, false);

            return arr;
        }

        public IReadOnlyDictionary<string, string> GetFileMetadata() => _basePtr->Metadata;

        public void Dispose()
        {
            if (_basePtr != null && _isOpen)
                avformat_close_input(ref _basePtr);
            else if (_basePtr != null)
                avformat_free_context(ref _basePtr);
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
        private static extern AvError avformat_find_stream_info(AvFormatContextStruct* context, void* options);

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
