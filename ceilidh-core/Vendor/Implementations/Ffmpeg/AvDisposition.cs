using System;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    [Flags]
    internal enum AvDisposition
    {
        Default = 0x1,
        Dub = 0x2,
        Original = 0x4,
        Comment = 0x8,
        Lyrics = 0x10,
        Karaoke = 0x20,
        Forced = 0x40,
        HearingImpaired = 0x80,
        VisualImpaired = 0x100,
        CleanEffects = 0x200,
        AttachedPic = 0x400,
        TimedThumbnails = 0x800,
        Captions = 0x1000,
        Descriptions = 0x2000,
        Metadata = 0x4000,
        Dependent = 0x8000,
        StillImage = 0x10000
    }
}