using Ceilidh.Core.Util;
using Ceilidh.Core.Vendor.Contracts;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations
{
    public sealed class LibAvDecoder : IDecoder
    {
        private readonly bool _supported;

        public LibAvDecoder()
        {
            try
            {
                AvFormatContext.RegisterAllFormats();
                _supported = true;
            }
            catch(TypeLoadException)
            {
                _supported = false;
            }
        }

        public bool TryDecode(Stream source, out AudioStream audioData)
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

                    if (format.OpenInput() != AvError.Ok)
                    {
                        format.Dispose();
                        io.Dispose();
                        return false;
                    }

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

    internal unsafe class AvIoContext : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ReadWritePacketHandler(IntPtr opaque, byte* buf, int bufSize);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long SeekHandler(IntPtr opaque, long offset, int whence);

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

#pragma warning restore IDE1006

        #endregion
    }

    internal unsafe class AvFormatContext : IDisposable
    {
        private bool _isOpen;
        private AvFormatContextStruct* _basePtr;
        private readonly AvIoContext _context;

        public AvFormatContext(AvIoContext ioContext)
        {
            _basePtr = avformat_alloc_context();
            _basePtr->pb = ioContext.BasePointer;

            _context = ioContext;
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
            private void* av_class;
            private void* iformat;
            private void* oformat;
            private void* priv_data;
            public void* pb;
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
