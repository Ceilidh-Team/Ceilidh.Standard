using System;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly unsafe struct AvPacket
    {
        public readonly void* Buffer;
        public readonly long PresentationTimestamp;
        public readonly long DecompressionTimestamp;
        public readonly byte* Data;
        public readonly int Size;
        public readonly int StreamIndex;
        public readonly int Flags;
        public readonly void* SideData;
        public readonly int SideDataElements;
        public readonly long Duration;
        public readonly long Position;
        [Obsolete]
        public readonly long ConvergenceDuration;
    }
}