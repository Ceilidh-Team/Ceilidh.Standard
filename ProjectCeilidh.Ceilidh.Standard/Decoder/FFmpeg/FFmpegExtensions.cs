using System;
using FFmpeg.AutoGen;
using ProjectCeilidh.Ceilidh.Standard.Audio;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg
{
    internal static class FFmpegExtensions
    {
        public static AVSampleFormat GetSampleFormat(this AudioDataFormat dataFormat)
        {
            switch (dataFormat.BytesPerSample)
            {
                case 1 when dataFormat.NumberFormat == NumberFormat.Unsigned:
                    return AVSampleFormat.AV_SAMPLE_FMT_U8;
                case 2 when dataFormat.NumberFormat == NumberFormat.Signed:
                    return AVSampleFormat.AV_SAMPLE_FMT_S16;
                case 4 when dataFormat.NumberFormat == NumberFormat.FloatingPoint:
                    return AVSampleFormat.AV_SAMPLE_FMT_FLT;
                case 4 when dataFormat.NumberFormat == NumberFormat.Signed:
                    return AVSampleFormat.AV_SAMPLE_FMT_S32;
                case 8 when dataFormat.NumberFormat == NumberFormat.FloatingPoint:
                    return AVSampleFormat.AV_SAMPLE_FMT_DBL;
                case 8 when dataFormat.NumberFormat == NumberFormat.Signed:
                    return AVSampleFormat.AV_SAMPLE_FMT_S64;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
