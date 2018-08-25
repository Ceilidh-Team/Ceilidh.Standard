using System;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal enum AvMediaType
    {
        Unknown = -1,
        Video = 0,
        Audio = 1,
        Data = 2,
        Subtitle = 3,
        Attachment = 4,
        Nb = 5
    }

    internal static class AvMediaTypeExtensions
    {
        public static string MediaTypeString(this AvMediaType type) => Marshal.PtrToStringUTF8(av_get_media_type_string(type));

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern IntPtr av_get_media_type_string(AvMediaType type);
    }
}
