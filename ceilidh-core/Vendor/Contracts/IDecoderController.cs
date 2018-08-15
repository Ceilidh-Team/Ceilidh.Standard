namespace Ceilidh.Core.Vendor.Contracts
{
    public interface IDecoderController
    {
        bool TryDecode(LowTrack track, out AudioStream audioData);
    }
}
