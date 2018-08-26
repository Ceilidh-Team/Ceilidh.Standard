using System;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AvPacket
    {
        public void* Buffer;
        public long PresentationTimestamp;
        public long DecompressionTimestamp;
        public byte* Data;
        public int Size;
        public int StreamIndex;
        public int Flags;
        public void* SideData;
        public int SideDataElements;
        public long Duration;
        public long Position;
        [Obsolete]
        public long ConvergenceDuration;
    }
}