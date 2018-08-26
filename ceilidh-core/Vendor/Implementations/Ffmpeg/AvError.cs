using System;
using System.Runtime.InteropServices;
using System.Text;
using Ceilidh.Core.Util;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal enum AvError
    {
        Ok = 0,
        /// <summary>
        /// Bitstream filter not found.
        /// </summary>
        BsfNotFound = -(0xF8 | ('B' << 8) | ('S' << 16) | ('F' << 24)),
        /// <summary>
        /// Internal bug
        /// </summary>
        /// <seealso cref="Bug2"/>
        Bug = -('B' | ('U' << 8) | ('G' << 16) | ('!' << 24)),
        /// <summary>
        /// Buffer too small
        /// </summary>
        BufferTooSmall = -('B' | ('U' << 8) | ('F' << 16) | ('S' << 24)),
        /// <summary>
        /// Decoder not found
        /// </summary>
        DecoderNotFound = -(0xF8 | ('D' << 8) | ('E' << 16) | ('C' << 24)),
        /// <summary>
        /// Demuxer not found
        /// </summary>
        DemuxerNotFound = -(0xF8 | ('D' << 8) | ('E' << 16) | ('M' << 24)),
        /// <summary>
        /// Encoder not found
        /// </summary>
        EncoderNotFound = -(0xF8 | ('E' << 8) | ('N' << 16) | ('C' << 24)),
        /// <summary>
        /// End of file
        /// </summary>
        Eof = -('E' | ('O' << 8) | ('F' << 16) | (' ' << 24)),
        /// <summary>
        /// Immediate exit was requested; the called function should not be restarted
        /// </summary>
        Exit = -('E' | ('X' << 8) | ('I' << 16) | ('T' << 24)),
        /// <summary>
        /// Generic error in an external library
        /// </summary>
        External = -('E' | ('X' << 8) | ('T' << 16) | (' ' << 24)),
        /// <summary>
        /// Filter not found
        /// </summary>
        FilterNotFound = -(0xF8 | ('F' << 8) | ('I' << 16) | ('L' << 24)),
        /// <summary>
        /// Invalid data found when processing input
        /// </summary>
        InvalidData = -('I' | ('N' << 8) | ('D' << 16) | ('A' << 24)),
        /// <summary>
        /// Muxer not found
        /// </summary>
        MuxerNotFound = -(0xF8 | ('M' << 8) | ('U' << 16) | ('X' << 24)),
        /// <summary>
        /// Option not found
        /// </summary>
        OptionNotFound = -(0xF8 | ('O' << 8) | ('P' << 16) | ('T' << 24)),
        /// <summary>
        /// Not yet implemented in FFmpeg, patches welcome
        /// </summary>
        PatchWelcome = -('P' | ('A' << 8) | ('W' << 16) | ('E' << 24)),
        /// <summary>
        /// Protocol not found
        /// </summary>
        ProtocolNotFound = -(0xF8 | ('P' << 8) | ('R' << 16) | ('O' << 24)),
        /// <summary>
        /// Stream not found
        /// </summary>
        StreamNotFound = -(0xF8 | ('S' << 8) | ('T' << 16) | ('R' << 24)),
        /// <summary>
        /// This is semantically identical to <see cref="Bug"/>. It has been introduced in Libav after our Bug and with a modified value.
        /// </summary>
        Bug2 = -('B' | ('U' << 8) | ('G' << 16) | (' ' << 24)),
        /// <summary>
        /// Unknown error, typically from an external library
        /// </summary>
        Unknown = -('U' | ('N' << 8) | ('K' << 16) | ('N' << 24)),
        /// <summary>
        /// Requested feature is experimental. Set strict_std_compliance if you really want to use it.
        /// </summary>
        Experimental = -0x2bb2afa8,
        /// <summary>
        /// Input changed between calls. Reconfiguration is required.
        /// </summary>
        InputChanged = -0x636e6701,
        /// <summary>
        /// Output changed between calls. Reconfiguration is required.
        /// </summary>
        OutputChanged = -0x636e6702,
        InputOutputChanged = InputChanged | OutputChanged,
        HttpBadRequest = -(0xF8 | ('4' << 8) | ('0' << 16) | ('0' << 24)),
        HttpUnauthorized = -(0xF8 | ('4' << 8) | ('0' << 16) | ('1' << 24)),
        HttpForbidden = -(0xF8 | ('4' << 8) | ('0' << 16) | ('3' << 24)),
        HttpNotFound = -(0xF8 | ('4' << 8) | ('0' << 16) | ('4' << 24)),
        HttpOther4Xx = -(0xF8 | ('4' << 8) | ('X' << 16) | ('X' << 24)),
        HttpServerError = -(0xF8 | ('5' << 8) | ('X' << 16) | ('X' << 24)),

        /// <summary>
        /// Try again
        /// </summary>
        EAgain = -11,
        /// <summary>
        /// Out of memory
        /// </summary>
        ENoMem = -12,
        /// <summary>
        /// Invalid argument
        /// </summary>
        EInval = -22
    }

    internal static unsafe class AvErrorExtensions
    {
        private const int MaxErrorStringSize = 64;
        
        /// <summary>
        /// Get the human-readable description string for this error code
        /// </summary>
        /// <param name="err">The error code to get data for</param>
        /// <returns>The human-readable description string, null if not found</returns>
        public static string GetErrorString(this AvError err)
        {
            var buf = stackalloc byte[MaxErrorStringSize];
            return av_strerror(err, buf, MaxErrorStringSize) < 0 ? null : Marshal.PtrToStringUTF8(new IntPtr(buf), MaxErrorStringSize);
        }
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int av_strerror(AvError err, void* errBuf, NativeInt len);
    }
}