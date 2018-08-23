using System;
using System.IO;
using System.Runtime.InteropServices;
using Ceilidh.Core.Util;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AvIoContextStruct
    {
        public Span<byte> Buffer => new Span<byte>(_buffer, _bufferSize);
        public Span<byte> ActiveBufferSegment => new Span<byte>(_bufferPointer, (int)(_bufferEnd - _bufferPointer));

        public Stream Stream
        {
            get
            {
                if (_opaque == IntPtr.Zero)
                    return null;

                try
                {
                    var handle = GCHandle.FromIntPtr(_opaque);
                    return handle.IsAllocated ? handle.Target as Stream : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool MustFlush
        {
            get => _mustFlush != 0;
            set => _mustFlush = value ? 1 : 0;
        }

        public bool EofReached => _eofReached != 0;

        public bool WriteFlag => _writeFlag != 0;

        public bool Direct
        {
            get => _direct != 0;
            set => _direct = value ? 1 : 0;
        }

#pragma warning disable 169
#pragma warning disable 649

        public readonly void* AvClass;
        private readonly byte* _buffer;
        private readonly int _bufferSize;
        private readonly byte* _bufferPointer;
        private readonly byte* _bufferEnd;
        private readonly IntPtr _opaque;
        private readonly IntPtr _readPacket;
        private readonly IntPtr _writePacket;
        private readonly IntPtr _seek;
        public readonly long Position;
        private int _mustFlush;
        private readonly int _eofReached;

        private readonly int _writeFlag;
        public readonly int MaxPacketSize;
        private readonly long _checksum;
        private readonly byte* _checksumPtr;
        private readonly void* _updateChecksum;
        public AvError Error;
        private readonly void* _readPause;

        private readonly void* _readSeek;
        private readonly int _seekable;
        private readonly long _maxSize;
        private int _direct;

#pragma warning restore 649
#pragma warning restore 169
    }

    internal sealed unsafe class AvIoContext : Stream
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ReadWritePacketHandler(IntPtr opaque, byte* buf, int bufSize);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long SeekHandler(IntPtr opaque, long offset, int whence);

        private static readonly ReadWritePacketHandler ReadDelegate = ReadImpl;
        private static readonly ReadWritePacketHandler WriteDelegate = WriteImpl;
        private static readonly SeekHandler SeekDelegate = SeekImpl;

        private readonly bool _ownHandle;
        private byte* _buffer;
        private AvIoContextStruct* _basePtr;
        private GCHandle _streamHandle;

        /// <inheritdoc />
        /// <summary>
        /// Construct an AvIoContext from an already-allocated context
        /// </summary>
        /// <param name="basePtr">The pointer to the existing context</param>
        /// <param name="ownHandle">If true, this instance will free the AvIoContext when Dispose is called. This does not free the internal buffer</param>
        public AvIoContext(AvIoContextStruct* basePtr, bool ownHandle = true)
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
            _ownHandle = true;
            _streamHandle = GCHandle.Alloc(stream);

            _buffer = av_malloc(Environment.SystemPageSize);
            _basePtr = avio_alloc_context(_buffer, Environment.SystemPageSize, stream.CanWrite ? 1 : 0,
                GCHandle.ToIntPtr(_streamHandle), stream.CanRead ? ReadDelegate : null,
                stream.CanWrite ? WriteDelegate : null, stream.CanSeek ? SeekDelegate : null);
        }

        public ref AvIoContextStruct GetPinnableReference()
        {
            return ref *_basePtr;
        }

        protected override void Dispose(bool disposing)
        {
            if (_ownHandle)
            {

                if (_buffer != null)
                    av_freep(ref _buffer);
                if (_basePtr != null)
                    avio_context_free(ref _basePtr);
            }

            if (_streamHandle.IsAllocated)
                _streamHandle.Free();
        }

        public override void Flush()
        {
            avio_flush(_basePtr);
        }

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
                    whence = 0;
                    break;
                case SeekOrigin.Current:
                    whence = 1;
                    break;
                case SeekOrigin.End:
                    whence = 2;
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
                case 0:
                    return stream.Seek(offset, SeekOrigin.Begin);
                case 1:
                    return stream.Seek(offset, SeekOrigin.Current);
                case 2:
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
        private static extern AvIoContextStruct* avio_alloc_context(byte* buffer, int bufferSize, int writeFlag,
            IntPtr opaque, ReadWritePacketHandler readPacket, ReadWritePacketHandler writePacket, SeekHandler seek);

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern void avio_context_free(ref AvIoContextStruct* s);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern byte* av_malloc(NativeInt size);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif  
        private static extern void av_freep(ref byte* buffer);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern void avio_write(AvIoContextStruct* s, byte* buf, int size);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern long avio_seek(AvIoContextStruct* s, long offset, int whence);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int avio_read(AvIoContextStruct* s, void* buf, int size);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern long avio_size(AvIoContextStruct* s);
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern long avio_tell(AvIoContextStruct* s);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern void avio_flush(AvIoContextStruct* s);

        #endregion
    }
}