using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal enum AvSampleFormat
    {
        None = -1,
        /// <summary>
        /// Unsigned 8 bits
        /// </summary>
        U8 = 0,
        /// <summary>
        /// Signed 16 bits
        /// </summary>
        S16 = 1,
        /// <summary>
        /// Signed 32 bits
        /// </summary>
        S32 = 2,
        Float = 3,
        Double = 4,
        /// <summary>
        /// Unsigned 8 bits, planar
        /// </summary>
        U8P = 5,
        /// <summary>
        /// Signed 16 bits, planar
        /// </summary>
        S16P = 6,
        /// <summary>
        /// Signed 32 bits, planar
        /// </summary>
        S32P = 7,
        FloatPlanar = 8,
        DoublePlanar = 9,
    }

    internal static class AvSampleFormatExtensions
    {
        public static bool IsPlanar(this AvSampleFormat format) => av_sample_fmt_is_planar(format) != 0;
        public static int BytesPerSample(this AvSampleFormat format) => av_get_bytes_per_sample(format);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int av_sample_fmt_is_planar(AvSampleFormat format);

#if WIN32
        [DllImport("avutil-56")]
#else
        [DllImport("avutil")]
#endif
        private static extern int av_get_bytes_per_sample(AvSampleFormat format);
    }
}
