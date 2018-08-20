using Ceilidh.Core.Util;
using Ceilidh.Core.Vendor.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

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

                    var data = format.GetFileMetadata();

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
        public override int SteamCount { get; }
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
        [DllImport("avformat-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern void* avio_alloc_context(void* buffer, int bufferSize, int writeFlag,
            IntPtr opaque, ReadWritePacketHandler readPacket, ReadWritePacketHandler writePacket, SeekHandler seek);

#if WIN32
        [DllImport("avutil-54")]
#else
        [DllImport("avutil")]
#endif
        private static extern void* av_malloc(NativeInt size);

#if WIN32
        [DllImport("avutil-54")]
#else
        [DllImport("avutil")]
#endif  
        private static extern void av_freep(ref void* buffer);

#if WIN32
        [DllImport("avutil-54")]
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

        public List<IReadOnlyDictionary<string, string>> GetFileMetadata()
        {
            var l = new List<IReadOnlyDictionary<string, string>>();

            for (var i = 0; i < _basePtr->nb_streams; i++)
            {
                var data = new Dictionary<string, string>();

                var dict = _basePtr->streams[i]->metadata;

                AvDictionaryEntry* tag = null;
                while ((tag = av_dict_get(dict, "", tag, 2 /* AV_DICT_IGNORE_SUFFIX */)) != null)
                    data.Add(tag->Key, tag->Value);

                l.Add(data);
            }

            return l;
        }

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
            public char* url;
            public long start_time;
            public long duration;
            public int bit_rate;
            public uint packet_size;
            public int max_delay;
            public int flags;
            public uint probesize;
            public int max_analyze_duration;
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

        [StructLayout(LayoutKind.Sequential)]
        private struct AvDictionaryEntry
        {
            private readonly char* _key;
            private readonly char* _value;

            public string Key => new string(_key);
            public string Value => new string(_value);
        }

        #region Native

#pragma warning disable IDE1006

#if WIN32
        [DllImport("avformat-56")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvFormatContextStruct* avformat_alloc_context();

#if WIN32
        [DllImport("avformat-56")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvError avformat_open_input(ref AvFormatContextStruct* context, [MarshalAs(UnmanagedType.LPStr)] string url, void* fmt, void* options);

#if WIN32
        [DllImport("avformat-56")]
#else
        [DllImport("avformat")]
#endif
        private static extern void avformat_close_input(ref AvFormatContextStruct* context);

#if WIN32
        [DllImport("avformat-56")]
#else
        [DllImport("avformat")]
#endif
        private static extern void avformat_free_context(ref AvFormatContextStruct* context);

#if WIN32
        [DllImport("avformat-56")]
#else
        [DllImport("avformat")]
#endif
        private static extern int avformat_version();

#if WIN32
        [DllImport("avformat-56")]
#else
        [DllImport("avformat")]
#endif
        private static extern AvError avformat_find_stream_info(AvFormatContextStruct* context, void* options);

#if WIN32
        [DllImport("avcodec-56")]
#else
        [DllImport("avcodec")]
#endif
        private static extern int avcodec_version();

#if WIN32
        [DllImport("avutil-54")]
#else
        [DllImport("avutil")]
#endif
        private static extern AvDictionaryEntry* av_dict_get(void* dict, [MarshalAs(UnmanagedType.LPStr)] string key, AvDictionaryEntry* prevEntry, int flags);

#if WIN32
        [DllImport("avformat-56", EntryPoint = "av_register_all")]
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
