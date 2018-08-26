using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Ceilidh.Core.Util;

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
            get => _avIoContext == null ? null : new AvIoContext(_avIoContext, false);
            set
            {
                if (value == null)
                {
                    _avIoContext = null;
                    return;
                }

                fixed (AvIoContextStruct* ptr = value)
                    _avIoContext = ptr;
            }
        }

        public ReadOnlySpan<AvStreamReference> Streams => new ReadOnlySpan<AvStreamReference>(_streams, (int)_streamCount);

        public string Url => Marshal.PtrToStringUTF8(new IntPtr(_url), 1024);

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
        private readonly byte* _url;
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
        public readonly void** Chapters;
        private readonly AvDictionaryStruct* _metadata;

#pragma warning restore 169
#pragma warning restore 649
    }

    internal unsafe class AvFormatContext : IDisposable
    {
        public ReadOnlySpan<AvStreamReference> Streams => _basePtr->Streams;

        private bool _isOpen;
        private AvFormatContextStruct* _basePtr;
        private readonly AvIoContext _context;

        public AvFormatContext(AvIoContext ioContext)
        {
            _context = ioContext;

            _basePtr = avformat_alloc_context();
            _basePtr->AvIoContext = ioContext;
        }

        public ref AvFormatContextStruct GetPinnableReference()
        {
            return ref *_basePtr;
        }

        public AvError OpenInput(string url = "")
        {
            fixed (byte* ptr = Encoding.UTF8.GetBytesNullTerminated(url))
            {
                var code = avformat_open_input(ref _basePtr, ptr, null, null);
                if (code == AvError.Ok)
                    _isOpen = true;

                return code;
            }
        }

        public AvError FindStreamInfo() => avformat_find_stream_info(_basePtr, null);

        public IReadOnlyDictionary<string, string>[] GetStreamMetadata()
        {
            var streams = _basePtr->Streams;

             var arr = new IReadOnlyDictionary<string, string>[streams.Length];

            for (var i = 0; i < streams.Length; i++)
                arr[i] = streams[i].Stream.Metadata;

            return arr;
        }

        public IReadOnlyDictionary<string, string> GetFileMetadata() => _basePtr->Metadata;

        public void Dispose()
        {
            if (_basePtr == null) return;

            _context.Dispose();
            _basePtr->AvIoContext = null;

            if (_isOpen)
                avformat_close_input(ref _basePtr);
            else
            {
                avformat_free_context(_basePtr);
                _basePtr = null;
            }
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
        private static extern AvError avformat_open_input(ref AvFormatContextStruct* context, byte* url, void* fmt, void* options);

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
        private static extern void avformat_free_context(AvFormatContextStruct* context);

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
