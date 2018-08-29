using System;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [Flags]
    internal enum AvSeekFlag
    {
        Backward = 1,
        Byte = 2,
        Any = 4,
        Frame = 8
    }
}
