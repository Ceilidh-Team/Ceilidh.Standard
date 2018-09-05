using ProjectCeilidh.Ceilidh.Standard.Library;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    public interface IDecoderController
    {
        bool TryDecode(Source source, out AudioData audioData);
    }
}
