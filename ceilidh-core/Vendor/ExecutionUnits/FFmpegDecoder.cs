using System;
using System.IO;
using System.Runtime.InteropServices;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.ExecutionUnits
{
    public sealed unsafe class FFmpegDecoder : IDecoder
    {
        public FFmpegDecoder()
        {
            TryDecode(null, out _);
        }

        public bool TryDecode(Stream source, out AudioStream audioData)
        {
            const int bufSize = 4 * 1024;
            
            var buf = AvIoContext.AvMalloc(new IntPtr(bufSize));
            ref var a = ref AvIoContext.AllocContext(buf, bufSize, 0, null, source);
            try
            {
                
            }
            finally
            {
                AvIoContext.AvFree(buf);
                a.Free();
            }
            
            audioData = null;
            return false;
        }
        
        private struct AvIoContext
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ReadWritePacketHandler(void* opaque, byte[] buf, int bufSize);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate long SeekHandler(void* opaque, long offset, int whence);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate ulong UpdateChecksumHandler(ulong checksum, byte[] buf, uint size);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ReadPauseHandler(void* opaque, [MarshalAs(UnmanagedType.I4)] bool pause);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate ulong ReadSeekHandler(void* opaque, int streamIndex, ulong timestamp, int flags);
            
            private AvIoContext* ThisPointer
            {
                get
                {
                    fixed(AvIoContext* ptr = &this)
                        return ptr;
                }
            }

            /// <summary>
            /// A class for private options
            /// </summary>
            public readonly void* av_class;
            /// <summary>
            /// Start of the buffer
            /// </summary>
            public byte* buffer;
            /// <summary>
            /// Maximum buffer size
            /// </summary>
            public int bufferSize;
            /// <summary>
            /// Current position in the buffer
            /// </summary>
            public byte* buf_ptr;
            /// <summary>
            /// End of the data, may be less than buffer + buffer_size if the read function returned less data than requested, e.g.
            /// </summary>
            public byte* buf_end;
            /// <summary>
            /// A private pointer, passed to the read/write/seek/...
            /// </summary>
            public void* opaque;
            
            private void* _readPacket;
            private void* _writePacket;
            private void* _seek;

            /// <summary>
            /// Position in the file of the current buffer
            /// </summary>
            public long pos;
            /// <summary>
            /// True if the next seek should flush
            /// </summary>
            public int must_flush;
            /// <summary>
            /// True if eof reached.
            /// </summary>
            public int eof_reached;
            /// <summary>
            /// True if open for writing
            /// </summary>
            public int write_flag;
            public int max_packet_size;
            public ulong checksum;
            public byte* checksum_ptr;

            public UpdateChecksumHandler UpdateChecksum
            {
                set => _updateChecksum = Marshal.GetFunctionPointerForDelegate(value).ToPointer();
            }
            
            private void* _updateChecksum;
            
            /// <summary>
            /// Contains the error code or 0 if no error happened
            /// </summary>
            public int error;

            public ReadPauseHandler ReadPause
            {
                set => _readPause = Marshal.GetFunctionPointerForDelegate(value).ToPointer();
            }
            
            /// <summary>
            /// Pause or resume playback for network streaming protocols - e.g.
            /// </summary>
            private void* _readPause;

            public ReadSeekHandler ReadSeek
            {
                set => _readSeek = Marshal.GetFunctionPointerForDelegate(value).ToPointer();
            }
            
            /// <summary>
            /// Seek to a given timestamp in stream with the specified stream_index
            /// </summary>
            private void* _readSeek;
            
            
            /// <summary>
            /// A combination of AVIO_SEEKABLE_ flags or 0 when the stream is not seekable.
            /// </summary>
            public int seekable;
            /// <summary>
            /// Max filesize, used to limit allocations. This field is internal to libavformat and access from outside is not allowed.
            /// </summary>
            private ulong maxsize;
            /// <summary>
            /// avio_read and avio_write should if possible be satisfied directly instead of going through a buffer, and avio_seek will always call the underlying seek function directly. 
            /// </summary>
            public int direct;
            /// <summary>
            /// Bytes read statistic
            /// </summary>
            private ulong bytes_read;
            private int seek_count;
            private int writeout_count;
            private int orig_buffer_size;

            public void Free()
            {
                var ptr = ThisPointer;
                avio_context_free(ref ptr);
            }
            
            public static ref AvIoContext AllocContext(void* buffer, int bufferSize, int writeFlag, void* opaque, Stream stream)
            {
                return ref AllocContext(buffer, bufferSize, writeFlag, opaque, ReadPacket, null, Seek);

                int ReadPacket(void* op, byte[] buf, int bufSize) => stream.Read(buf, 0, bufSize);

                long Seek(void* op, long offset, int whence)
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
            
            [DllImport("avformat", EntryPoint = "avio_alloc_context")]
            private static extern ref AvIoContext AllocContext(void* buffer, int bufferSize, int writeFlag, void* opaque,
                ReadWritePacketHandler readPacket, ReadWritePacketHandler writePacket, SeekHandler seek);

            [DllImport("avformat", EntryPoint = "av_malloc")]
            public static extern void* AvMalloc(IntPtr size);

            [DllImport("avformat", EntryPoint = "av_freep")]
            public static extern void AvFree(void* buffer);

            [DllImport("avformat")]
            private static extern void avio_context_free(ref AvIoContext* s);
        }

        private struct AvFormatContext
        {
            [DllImport("avformat", EntryPoint = "avformat_alloc_context")]
            public static extern ref AvFormatContext AllocateContext();
        }
    }
}