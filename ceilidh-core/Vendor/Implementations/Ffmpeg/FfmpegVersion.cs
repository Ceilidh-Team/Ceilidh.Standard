using System;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal static class FfmpegVersion
    {
        public static Version AvFormatVersion => VersionForCode(avformat_version());

        public static Version AvCodecVersion => VersionForCode(avcodec_version());

        public static Version AvUtilVersion => VersionForCode(avutil_version());

        private static Version VersionForCode(int code) =>
            new Version((code >> 16) & 0xFF, (code >> 8) & 0xFF, code & 0xFF);

        #region Native

#if WIN32
        [DllImport("avcodec-58")]
#else
        [DllImport("avcodec")]
#endif
        private static extern int avcodec_version();

#if WIN32
        [DllImport("avformat-58")]
#else
        [DllImport("avformat")]
#endif
        private static extern int avformat_version();

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif  
        private static extern int avutil_version();

        #endregion
    }
}
