using Ceilidh.Core.Util;
using Ceilidh.Core.Vendor.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ceilidh.Core.Vendor.Implementations
{
    public sealed class LibAvDecoder : IDecoder
    {
        private readonly bool _supported;
        private readonly ILocalizationController _localization;

        public LibAvDecoder(ILocalizationController localization)
        {
            _localization = localization;

            try
            {
                AvFormatContext.RegisterAllFormats();

                Console.WriteLine(_localization.Translate("libav.util.version", AvIoContext.AvUtilVersion));
                Console.WriteLine(_localization.Translate("libav.format.version", AvFormatContext.AvFormatVersion));
                Console.WriteLine(_localization.Translate("libav.codec.version", AvFormatContext.AvCodecVersion));

                _supported = true;
            }
            catch(TypeLoadException)
            {
                Console.WriteLine(_localization.Translate("libav.disabled"));

                _supported = false;
            }
        }

        public bool TryDecode(Stream source, out AudioData audioData)
        {
            audioData = null;
            if (!_supported) return false;

            AvIoContext io = null;
            try
            {
                AvFormatContext format = null;

                io = new AvIoContext(source);
                try
                {
                    format = new AvFormatContext(io);

                    if (format.OpenInput() != AvError.Ok || format.FindStreamInfo() != AvError.Ok)
                    {
                        format.Dispose();
                        io.Dispose();
                        return false;
                    }

                    var data = format.GetStreamMetadata();

                    foreach (var stream in data)
                    foreach (var (key, value) in stream)
                        Console.WriteLine($"{key}: {value}");

                    return true;
                }
                catch
                {
                    format?.Dispose();
                    throw;
                }
            }
            catch
            {
                io?.Dispose();
                throw;
            }
        }
    }

    internal class LibAvAudioData : AudioData
    {
        public override IReadOnlyDictionary<string, string> Metadata { get; }
        public override int StreamCount { get; }
        public override int SelectedStream { get; }

        private readonly AvIoContext _ioContext;
        private readonly AvFormatContext _formatContext;

        public LibAvAudioData(AvIoContext io, AvFormatContext format)
        {
            _ioContext = io;
            _formatContext = format;
        }

        public override bool TrySelectStream(int streamIndex) => throw new NotImplementedException();

        public override AudioStream GetAudioStream() => throw new NotImplementedException();

        public override void Dispose()
        {
            _formatContext.Dispose();
            _ioContext.Dispose();
        }
    }

    internal unsafe class AvIoContext : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ReadWritePacketHandler(IntPtr opaque, byte* buf, int bufSize);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long SeekHandler(IntPtr opaque, long offset, int whence);

        public static Version AvUtilVersion
        {
            get
            {
                var ver = avutil_version();
                return new Version((ver >> 16) & 0xff, (ver >> 8) & 0xff, ver & 0xff);
            }
        }

        private static readonly ReadWritePacketHandler Read = ReadImpl;
        private static readonly SeekHandler Seek = SeekImpl;

        public void* BasePointer => _basePtr;

        private Stream _stream;
        private GCHandle _streamHandle;
        private void* _buffer;
        private void* _basePtr;

        public AvIoContext(Stream stream)
        {
            _stream = stream;

            _streamHandle = GCHandle.Alloc(_stream);

            _buffer = av_malloc(Environment.SystemPageSize);
            _basePtr = avio_alloc_context(_buffer, Environment.SystemPageSize, 0, GCHandle.ToIntPtr(_streamHandle), Read, null,
                stream.CanSeek ? Seek : null);
        }

        public void Dispose()
        {
            if (_buffer != null)
                av_freep(ref _buffer);
            if (_basePtr != null)
                av_freep(ref _basePtr);

            _stream.Dispose();
            _streamHandle.Free();
        }

        private static int ReadImpl(IntPtr opaque, byte* buf, int bufSize)
        {
            var stream = (Stream) GCHandle.FromIntPtr(opaque).Target;

            var data = new byte[bufSize];
            var len = stream.Read(data, 0, bufSize);
            if (len <= 0) return (int) AvError.Eof;

            fixed(byte* ptr = data)
                Buffer.MemoryCopy(ptr, buf, bufSize, len);

            return len;
        }

        private static long SeekImpl(IntPtr opaque, long offset, int whence)
        {
            var stream = (Stream)GCHandle.FromIntPtr(opaque).Target;

            switch (whence)
            {
                case 1:
                    return stream.Seek(offset, SeekOrigin.Begin);
                case 2:
                    return stream.Seek(offset, SeekOrigin.Current);
                case 3:
                    return stream.Seek(offset, SeekOrigin.End);
                case 0x10000:
                    return stream.Length;
                default:
                    return -1;
            }
        }

        #region Native

#pragma warning disable IDE1006

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern void* avio_alloc_context(void* buffer, int bufferSize, int writeFlag,
            IntPtr opaque, ReadWritePacketHandler readPacket, ReadWritePacketHandler writePacket, SeekHandler seek);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern void* av_malloc(NativeInt size);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif  
        private static extern void av_freep(ref void* buffer);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif  
        private static extern int avutil_version();

#pragma warning restore IDE1006

        #endregion
    }

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

        public long Duration => Enumerable.Range(0, (int)_basePtr->nb_streams).Select(x => _basePtr->streams[x]->duration).Max();

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
                arr[i] = _basePtr->streams[i]->metadata->CreateCopy();

            return arr;
        }

        public IReadOnlyDictionary<string, string> GetFileMetadata() => _basePtr->metadata->CreateCopy();

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
            public AvDictionary* metadata;
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
            public AvDictionary* metadata;
        }

        private struct AvDictionary
        {
            [Flags]
            private enum AvDictionaryFlags
            {
                MatchCase = 1,
                IgnoreSuffix = 2,
                DontStrdupKey = 4,
                DontStrdupVal = 8,
                DontOverwrite = 16,
                Append = 32
            }

            public IReadOnlyDictionary<string, string> CreateCopy()
            {
                var dict = new Dictionary<string, string>();

                fixed (AvDictionary* ptr = &this)
                fixed (byte* keyData = new byte[] { 0 })
                {
                    AvDictionaryEntry* entry = null;
                    while ((entry = av_dict_get(ptr, keyData, entry, AvDictionaryFlags.IgnoreSuffix)) != null)
                        dict.Add(entry->Key, entry->Value);
                }

                return dict;
            }

            public void Add(KeyValuePair<string, string> item) => Add(item.Key, item.Value);

            public bool Contains(KeyValuePair<string, string> item) => TryGetValue(item.Key, out var value) && item.Value == value;

            public bool Remove(KeyValuePair<string, string> item) => Remove(item.Key);

            public int Count
            {
                get
                {
                    fixed (AvDictionary* ptr = &this)
                        return av_dict_count(ptr);
                }
            }

            public bool IsReadOnly => false;

            public void Add(string key, string value)
            {
                fixed (AvDictionary* ptr = &this)
                fixed (byte* keyData = Encoding.UTF8.GetBytes(key))
                fixed (byte* valueData = Encoding.UTF8.GetBytes(value))
                {
                    var copy = ptr;
                    if (av_dict_set(ref copy, keyData, valueData, AvDictionaryFlags.DontOverwrite) < 0)
                        throw new ArgumentException("An item with the same key has already been added.", nameof(key));
                }
            }

            public bool ContainsKey(string key)
            {
                fixed (AvDictionary* ptr = &this)
                fixed (byte* keyData = Encoding.UTF8.GetBytes(key))
                    return av_dict_get(ptr, keyData, null, 0) != null;
            }

            public bool Remove(string key)
            {
                this[key] = null;
                return true;
            }

            public bool TryGetValue(string key, out string value)
            {
                fixed (AvDictionary* ptr = &this)
                fixed (byte* keyData = Encoding.UTF8.GetBytes(key))
                {
                    var entry = av_dict_get(ptr, keyData, null, 0);
                    if (entry == null)
                    {
                        value = null;
                        return false;
                    }

                    value = entry->Value;
                    return true;
                }
            }

            public string this[string key]
            {
                get
                {
                    fixed (AvDictionary* ptr = &this)
                    fixed (byte* keyData = Encoding.UTF8.GetBytes(key))
                    {
                        var entry = av_dict_get(ptr, keyData, null, 0);
                        return entry != null ? entry->Value : throw new KeyNotFoundException();
                    }
                }
                set
                {
                    fixed (AvDictionary* ptr = &this)
                    fixed (byte* keyData = Encoding.UTF8.GetBytes(key))
                    fixed (byte* valueData = Encoding.UTF8.GetBytes(value))
                    {
                        var copy = ptr;

                        av_dict_set(ref copy, keyData, valueData, 0);
                    }
                }
            }

            #region Native

#if WIN32
            [DllImport("avutil-56")]
#else
            [DllImport("avutil")]
#endif
            private static extern AvDictionaryEntry* av_dict_get(AvDictionary* m,
                byte* key, AvDictionaryEntry* prev, AvDictionaryFlags flags);

#if WIN32
            [DllImport("avutil-56")]
#else
            [DllImport("avutil")]
#endif
            private static extern int av_dict_count(AvDictionary* m);

#if WIN32
            [DllImport("avutil-56")]
#else
            [DllImport("avutil")]
#endif
            private static extern int av_dict_set(ref AvDictionary* pm, byte* key,
                byte* value, AvDictionaryFlags flags);

#if WIN32
            [DllImport("avutil-56")]
#else
            [DllImport("avutil")]
#endif
            private static extern void av_dict_copy(ref AvDictionary* dst, AvDictionary* src, AvDictionaryFlags flags);

#if WIN32
            [DllImport("avutil-56")]
#else
            [DllImport("avutil")]
#endif
            private static extern void av_dict_free(ref AvDictionary* m);

            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AvDictionaryEntry
        {
            private readonly IntPtr _key;
            private readonly IntPtr _value;

            public string Key => Marshal.PtrToStringUTF8(_key);
            public string Value => Marshal.PtrToStringUTF8(_value);
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

    internal enum AvError
    {
        Ok = 0,
        BsfNotFound = -0x39acbd08,
        DecoderNotFound = -0x3cbabb08,
        DemuxerNotFound = -0x32babb08,
        EncoderNotFound = -0x3cb1ba08,
        Eof = -0x5fb9b0bb,
        InvalidData = -0x3ebbb1b7,
    }
}
