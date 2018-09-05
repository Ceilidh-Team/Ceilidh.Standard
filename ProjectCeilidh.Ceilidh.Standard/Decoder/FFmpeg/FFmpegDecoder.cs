﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using static FFmpeg.AutoGen.ffmpeg;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg
{
    [CobbleExport]
    public unsafe class FFmpegDecoder : IDecoder
    {
        private static readonly avio_alloc_context_read_packet ReadDelegate = ReadImpl;
        private static readonly avio_alloc_context_seek SeekDelegate = SeekImpl;

        public FFmpegDecoder()
        {
            // TODO: Locate libraries on linux
        }

        public bool TryDecode(Stream source, out AudioData audioData)
        {
            audioData = default;

            AVIOContext* ioContext = null;
            try
            {
                ioContext = CreateIoContext(source, out var sourceHandle);

                AVFormatContext* formatContext = null;
                try
                {
                    formatContext = avformat_alloc_context();
                    formatContext->pb = ioContext;

                    if (avformat_open_input(&formatContext, "", null, null) != 0)
                    {
                        source.Dispose();
                        sourceHandle.Free();

                        if (formatContext != null)
                        {
                            av_freep(formatContext->pb->buffer);

                            avio_context_free(&formatContext->pb);
                            avformat_free_context(formatContext);
                        }

                        return false;
                    }

                    if (avformat_find_stream_info(formatContext, null) != 0)
                    {
                        source.Dispose();
                        sourceHandle.Free();

                        if (formatContext != null)
                        {
                            av_freep(formatContext->pb->buffer);

                            avio_context_free(&formatContext->pb);
                            avformat_close_input(&formatContext);
                        }

                        return false;
                    }

                    audioData = new FFmpegAudioData(formatContext);
                    ioContext = null;
                    formatContext = null;
                    return true;
                }
                catch
                {
                    // TODO
                    ioContext = null;

                    throw;
                }
            }
            catch
            {
                // TODO
                throw;
            }
        }

        private static AVIOContext* CreateIoContext(Stream stream, out GCHandle handle)
        {
            handle = GCHandle.Alloc(stream);

            var buffer = (byte*) av_malloc((ulong) Environment.SystemPageSize);

            return avio_alloc_context(buffer, Environment.SystemPageSize, 0, GCHandle.ToIntPtr(handle).ToPointer(), ReadDelegate, null, stream.CanSeek ? SeekDelegate : null);
        }

        private static int ReadImpl(void* opaque, byte* buf, int bufSize)
        {
            var stream = (Stream) GCHandle.FromIntPtr(new IntPtr(opaque)).Target;

            var managedBuffer = new byte[bufSize];

            var len = stream.Read(managedBuffer, 0, bufSize);

            fixed (byte* manBuf = managedBuffer)
                Buffer.MemoryCopy(manBuf, buf, bufSize, bufSize);

            return len <= 0 ? AVERROR_EOF : len;
        }

        private static long SeekImpl(void* opaque, long offset, int whence)
        {
            var stream = (Stream) GCHandle.FromIntPtr(new IntPtr(opaque)).Target;

            switch(whence)
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
    }
}
