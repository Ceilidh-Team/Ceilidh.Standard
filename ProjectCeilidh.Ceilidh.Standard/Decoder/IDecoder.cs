using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    public interface IDecoder
    {
        bool TryDecode(Stream source, out IAudioData audioData);
    }
}
