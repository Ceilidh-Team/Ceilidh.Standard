namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal enum AvDiscard
    {
        None = -16,
        Default = 0,
        NonReference = 8,
        BiDirectional = 16,
        NonIntra = 24,
        NonKeyframe = 32,
        All = 48
    }
}