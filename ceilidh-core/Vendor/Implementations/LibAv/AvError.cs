using System;
using System.Runtime.InteropServices;
using System.Text;
using Ceilidh.Core.Util;

namespace Ceilidh.Core.Vendor.Implementations.LibAv
{
    internal enum AvError
    {
        Ok = 0,
        BsfNotFound = -(0xF8 | ('B' << 8) | ('S' << 16) | ('F' << 24)),
        Bug = -('B' | ('U' << 8) | ('G' << 16) | ('!' << 24)),
        BufferTooSmall = -('B' | ('U' << 8) | ('F' << 16) | ('S' << 24)),
        DecoderNotFound = -(0xF8 | ('D' << 8) | ('E' << 16) | ('C' << 24)),
        DemuxerNotFound = -(0xF8 | ('D' << 8) | ('E' << 16) | ('M' << 24)),
        EncoderNotFound = -(0xF8 | ('E' << 8) | ('N' << 16) | ('C' << 24)),
        Eof = -('E' | ('O' << 8) | ('F' << 16) | (' ' << 24)),
        Exit = -('E' | ('X' << 8) | ('I' << 16) | ('T' << 24)),
        External = -('E' | ('X' << 8) | ('T' << 16) | (' ' << 24)),
        FilterNotFound = -(0xF8 | ('F' << 8) | ('I' << 16) | ('L' << 24)),
        InvalidData = -('I' | ('N' << 8) | ('D' << 16) | ('A' << 24)),
        MuxerNotFound = -(0xF8 | ('M' << 8) | ('U' << 16) | ('X' << 24)),
        OptionNotFound = -(0xF8 | ('O' << 8) | ('P' << 16) | ('T' << 24)),
        PatchWelcome = -('P' | ('A' << 8) | ('W' << 16) | ('E' << 24)),
        ProtocolNotFound = -(0xF8 | ('P' << 8) | ('R' << 16) | ('O' << 24)),
        StreamNotFound = -(0xF8 | ('S' << 8) | ('T' << 16) | ('R' << 24)),
        Bug2 = -('B' | ('U' << 8) | ('G' << 16) | (' ' << 24)),
        Unknown = -('U' | ('N' << 8) | ('K' << 16) | ('N' << 24)),
        Experimental = -0x2bb2afa8,
        InputChanged = -0x636e6701,
        OutputChanged = -0x636e6702,
        HttpBadRequest = -(0xF8 | ('4' << 8) | ('0' << 16) | ('0' << 24)),
        HttpUnauthorized = -(0xF8 | ('4' << 8) | ('0' << 16) | ('1' << 24)),
        HttpForbidden = -(0xF8 | ('4' << 8) | ('0' << 16) | ('3' << 24)),
        HttpNotFound = -(0xF8 | ('4' << 8) | ('0' << 16) | ('4' << 24)),
        HttpOther4Xx = -(0xF8 | ('4' << 8) | ('X' << 16) | ('X' << 24)),
        HttpServerError = -(0xF8 | ('5' << 8) | ('X' << 16) | ('X' << 24)),
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
            return av_strerror(err, buf, MaxErrorStringSize) < 0 ? null : Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buf, MaxErrorStringSize)).TrimEnd('\0');
        }
        
#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int av_strerror(AvError err, void* errBuf, NativeInt len);
    }
}