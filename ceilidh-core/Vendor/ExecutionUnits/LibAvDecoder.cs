using Ceilidh.Core.Util;
using Ceilidh.Core.Vendor.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.ExecutionUnits
{
    public sealed unsafe class LibAvDecoder : IDecoder
    {
        private readonly LibAvNative _native;
        private readonly bool _supported;

        public LibAvDecoder()
        {
            _native = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? (LibAvNative) new Win32LibAvNative()
                : new UnixLibAvNative();

            try
            {
                _native.AvRegisterAll();
                _supported = true;
            }
            catch(TypeLoadException)
            {
                _supported = false;
            }
        }

        public bool TryDecode(Stream source, out AudioStream audioData)
        {
            const int bufSize = 4 * 1024;

            audioData = null;
            if (!_supported) return false;

            var buf = _native.AvMalloc(bufSize);
            var io = _native.AvIoAllocContext(buf, bufSize, 0, source);

            var context = _native.AvFormatAllocContext();
            context->pb = io;

            var code = _native.AvFormatOpenInput(ref context, null, null, null);

            if (code == LibAvNative.AvError.Ok)
            {
                code = (LibAvNative.AvError) _native.AvFormatFindStreamInfo(context, null);
            }
            else
            {
                _native.AvFreeP(ref buf);
                _native.AvIoFreeContext(ref io);
            }

            audioData = null;
            return false;
        }

        private class AvFormatAudioStream : AudioStream
        {
            public AvFormatAudioStream(Stream stream, AudioFormat format) : base(stream, format)
            {
            }

            protected override void Dispose(bool disposing)
            {


                base.Dispose(disposing);
            }
        }
    }

    internal abstract unsafe class LibAvNative
    {
        private static readonly List<Delegate> LiveForever = new List<Delegate>();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int ReadWritePacketHandler(void* opaque, byte* buf, int bufSize);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate long SeekHandler(void* opaque, long offset, int whence); 

        public abstract void* AvMalloc(NativeInt size);
        protected abstract AvIoContext* AvIoAllocContext(void* buffer, int bufferSize, int writeFlag, void* opaque, ReadWritePacketHandler readPacket, ReadWritePacketHandler writePacket, SeekHandler seek);
        public abstract AvFormatContext* AvFormatAllocContext();
        public abstract AvError AvFormatOpenInput(ref AvFormatContext* context, char* url, void* fmt, void* options);
        public abstract int AvFormatFindStreamInfo(AvFormatContext* context, void* options);
        public abstract void AvFreeP(ref void* buffer);
        public abstract void AvFormatCloseInput(ref AvFormatContext* context);
        public abstract void AvFormatFreeContext(ref AvFormatContext* context);
        public abstract void AvRegisterAll();
        public abstract void* AvFindInputFormat(string shortName);
        protected abstract uint AvCodecVersionImpl();
        protected abstract uint AvFormatVersionImpl();

        public void AvIoFreeContext(ref AvIoContext* context)
        {
            var p = (void*) context;
            AvFreeP(ref p);
            context = (AvIoContext*) p;
        }

        public AvIoContext* AvIoAllocContext(void* buffer, int bufferSize, int writeFlag, Stream stream)
        {
            ReadWritePacketHandler read = ReadPacket;
            SeekHandler seek = Seek;

            LiveForever.Add(read);
            LiveForever.Add(seek);

            return AvIoAllocContext(buffer, bufferSize, writeFlag, null, read, null, stream.CanSeek ? seek : null);

            int ReadPacket(void* opaque, byte* buf, int bufSize)
            {
                var data = new byte[bufSize];
                var ret = stream.Read(data, 0, bufSize);
                if (ret <= 0)
                {
                    Console.WriteLine("EOF");
                    return (int) AvError.Eof;
                }

                Console.WriteLine(ret);

                fixed (byte* ptr = data)
                    Buffer.MemoryCopy(ptr, buf, bufSize, ret);

                return ret;
            }

            long Seek(void* opaque, long offset, int whence)
            {
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
        }

        public Version AvCodecVersion()
        {
            var ver = AvCodecVersionImpl();
            return new Version((int) ((ver >> 16) & 0xff), (int) ((ver >> 8) & 0xff), (int) (ver & 0xff));
        }

        public Version AvFormatVersion()
        {
            var ver = AvFormatVersionImpl();
            return new Version((int)((ver >> 16) & 0xff), (int)((ver >> 8) & 0xff), (int)(ver & 0xff));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AvFormatContext
        {
            public void* av_class;
            public void* iformat;
            public void* oformat;
            public void* priv_data;
            public void* pb;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AvIoContext
        {

        }

        public enum AvError
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

    internal unsafe class UnixLibAvNative : LibAvNative
    {
#pragma warning disable IDE1006 // Naming Styles

        [DllImport("avutil")]
        private static extern void* av_malloc(NativeInt size);
        [DllImport("avformat")]
        private static extern AvIoContext* avio_alloc_context(void* buffer, int bufferSize, int writeFlag, void* opaque, ReadWritePacketHandler readPacket, ReadWritePacketHandler writePacket, SeekHandler seek);
        [DllImport("avformat")]
        private static extern AvFormatContext* avformat_alloc_context();
        [DllImport("avformat")]
        private static extern AvError avformat_open_input(ref AvFormatContext* context, char* url, void* fmt, void* options);
        [DllImport("avformat")]
        private static extern int avformat_find_stream_info(AvFormatContext* context, void* options);
        [DllImport("avutil")]
        private static extern void av_freep(ref void* buffer);
        [DllImport("avformat")]
        private static extern void avformat_close_input(ref AvFormatContext* context);
        [DllImport("avformat")]
        private static extern void avformat_free_context(ref AvFormatContext* context);
        [DllImport("avcodec")]
        private static extern uint avcodec_version();
        [DllImport("avformat")]
        private static extern uint avformat_version();
        [DllImport("avformat")]
        private static extern void av_register_all();
        [DllImport("avformat")]
        private static extern void* av_find_input_format([MarshalAs(UnmanagedType.LPStr)] string shortName);

#pragma warning restore IDE1006 // Naming Styles

        public override void* AvMalloc(NativeInt size) => av_malloc(size);

        protected override AvIoContext* AvIoAllocContext(void* buffer, int bufferSize, int writeFlag, void* opaque, ReadWritePacketHandler readPacket,
            ReadWritePacketHandler writePacket, SeekHandler seek) =>
            avio_alloc_context(buffer, bufferSize, writeFlag, opaque, readPacket, writePacket, seek);

        public override AvFormatContext* AvFormatAllocContext() => avformat_alloc_context();

        public override AvError AvFormatOpenInput(ref AvFormatContext* context, char* url, void* fmt, void* options) =>
            avformat_open_input(ref context, url, fmt, options);

        public override int AvFormatFindStreamInfo(AvFormatContext* context, void* options) =>
            avformat_find_stream_info(context, options);

        public override void AvFreeP(ref void* buffer) => av_freep(ref buffer);

        public override void AvFormatCloseInput(ref AvFormatContext* context) => avformat_close_input(ref context);

        public override void AvFormatFreeContext(ref AvFormatContext* context) => avformat_free_context(ref context);
        protected override uint AvCodecVersionImpl() => avcodec_version();

        protected override uint AvFormatVersionImpl() => avformat_version();

        public override void AvRegisterAll() => av_register_all();

        public override void* AvFindInputFormat(string shortName) => av_find_input_format(shortName);
    }

    internal unsafe class Win32LibAvNative : LibAvNative
    {
#pragma warning disable IDE1006 // Naming Styles

        [DllImport("avutil-54")]
        private static extern void* av_malloc(NativeInt size);
        [DllImport("avformat-56")]
        private static extern AvIoContext* avio_alloc_context(void* buffer, int bufferSize, int writeFlag, void* opaque, ReadWritePacketHandler readPacket, ReadWritePacketHandler writePacket, SeekHandler seek);
        [DllImport("avformat-56")]
        private static extern AvFormatContext* avformat_alloc_context();
        [DllImport("avformat-56")]
        private static extern AvError avformat_open_input(ref AvFormatContext* context, char* url, void* fmt, void* options);
        [DllImport("avformat-56")]
        private static extern int avformat_find_stream_info(AvFormatContext* context, void* options);
        [DllImport("avutil-54")]
        private static extern void av_freep(ref void* buffer);
        [DllImport("avformat-56")]
        private static extern void avformat_close_input(ref AvFormatContext* context);
        [DllImport("avformat-56")]
        private static extern void avformat_free_context(ref AvFormatContext* context);
        [DllImport("avcodec-54")]
        private static extern uint avcodec_version();
        [DllImport("avformat-56")]
        private static extern uint avformat_version();
        [DllImport("avformat-56")]
        private static extern void av_register_all();
        [DllImport("avformat-56")]
        private static extern void* av_find_input_format([MarshalAs(UnmanagedType.LPStr)] string shortName);

#pragma warning restore IDE1006 // Naming Styles

        public override void* AvMalloc(NativeInt size) => av_malloc(size);

        protected override AvIoContext* AvIoAllocContext(void* buffer, int bufferSize, int writeFlag, void* opaque, ReadWritePacketHandler readPacket,
            ReadWritePacketHandler writePacket, SeekHandler seek) =>
            avio_alloc_context(buffer, bufferSize, writeFlag, opaque, readPacket, writePacket, seek);

        public override AvFormatContext* AvFormatAllocContext() => avformat_alloc_context();

        public override AvError AvFormatOpenInput(ref AvFormatContext* context, char* url, void* fmt, void* options) =>
            avformat_open_input(ref context, url, fmt, options);

        public override int AvFormatFindStreamInfo(AvFormatContext* context, void* options) =>
            avformat_find_stream_info(context, options);

        public override void AvFreeP(ref void* buffer) => av_freep(ref buffer);

        public override void AvFormatCloseInput(ref AvFormatContext* context) => avformat_close_input(ref context);

        public override void AvFormatFreeContext(ref AvFormatContext* context) => avformat_free_context(ref context);
        protected override uint AvCodecVersionImpl() => avcodec_version();

        protected override uint AvFormatVersionImpl() => avformat_version();

        public override void AvRegisterAll() => av_register_all();

        public override void* AvFindInputFormat(string shortName) => av_find_input_format(shortName);
    }
}
