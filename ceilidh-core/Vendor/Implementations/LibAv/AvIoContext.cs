﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Ceilidh.Core.Util;

namespace Ceilidh.Core.Vendor.Implementations.LibAv
{
    
    internal sealed unsafe class AvIoContext : Stream
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ReadWritePacketHandler(IntPtr opaque, byte* buf, int bufSize);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long SeekHandler(IntPtr opaque, long offset, int whence);

        private static readonly ReadWritePacketHandler ReadDelegate = ReadImpl;
        private static readonly ReadWritePacketHandler WriteDelegate = WriteImpl;
        private static readonly SeekHandler SeekDelegate = SeekImpl;

        public static Version AvUtilVersion
        {
            get
            {
                var ver = avutil_version();
                return new Version((ver >> 16) & 0xFF, (ver >> 8) & 0xFF, ver & 0xFF);
            }
        }
        
        public void* BasePointer => _basePtr;
        
        private readonly bool _ownHandle;
        private void* _buffer;
        private void* _basePtr;
        private GCHandle _streamHandle;

        /// <inheritdoc />
        /// <summary>
        /// Construct an AvIoContext from an already-allocated context
        /// </summary>
        /// <param name="basePtr">The pointer to the existing context</param>
        /// <param name="ownHandle">If true, this instance will free the AvIoContext when Dispose is called. This does not free the internal buffer</param>
        public AvIoContext(void* basePtr, bool ownHandle = true)
        {
            _basePtr = basePtr;
            _ownHandle = ownHandle;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Construct a readable AvIoContext from a given stream
        /// </summary>
        /// <param name="stream"></param>
        public AvIoContext(Stream stream)
        {
            _streamHandle = GCHandle.Alloc(stream);

            _buffer = av_malloc(Environment.SystemPageSize);
            _basePtr = avio_alloc_context(_buffer, Environment.SystemPageSize, stream.CanWrite ? 1 : 0,
                GCHandle.ToIntPtr(_streamHandle), stream.CanRead ? ReadDelegate : null, stream.CanWrite ? WriteDelegate : null, stream.CanSeek ? SeekDelegate : null);
        }

        protected override void Dispose(bool disposing)
        {
            if (_buffer != null)
                av_freep(ref _buffer);
            if (_basePtr != null)
                av_freep(ref _basePtr);
            if (_streamHandle.IsAllocated)
                _streamHandle.Free();
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            fixed (byte* buf = buffer)
                return avio_read(_basePtr, buf, buffer.Length);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            int whence;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    whence = 1;
                    break;
                case SeekOrigin.Current:
                    whence = 2;
                    break;
                case SeekOrigin.End:
                    whence = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }
            
            return avio_seek(_basePtr, offset, whence);
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            fixed(byte* ptr = buffer)
                avio_write(_basePtr, ptr, buffer.Length);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => avio_size(_basePtr);
        public override long Position
        {
            get => avio_tell(_basePtr);
            set => throw new NotSupportedException();
        }

        private static int ReadImpl(IntPtr handle, byte* buf, int bufSize)
        {
            var stream = (Stream) GCHandle.FromIntPtr(handle).Target;

            var len = stream.Read(new Span<byte>(buf, bufSize));
            
            return len <= 0 ? (int) AvError.Eof : len;
        }

        private static int WriteImpl(IntPtr handle, byte* buf, int bufSize)
        {
            var stream = (Stream) GCHandle.FromIntPtr(handle).Target;

            stream.Write(new ReadOnlySpan<byte>(buf, bufSize));
            return bufSize;
        }
        
        private static long SeekImpl(IntPtr handle, long offset, int whence)
        {
            var stream = (Stream) GCHandle.FromIntPtr(handle).Target;

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
        private static extern void avio_write(void* s, byte* buf, int size);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern long avio_seek(void* s, long offset, int whence);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int avio_read(void* s, void* buf, int size);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern long avio_size(void* s);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern long avio_tell(void* s);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif  
        private static extern int avutil_version();

        #endregion
    }
}