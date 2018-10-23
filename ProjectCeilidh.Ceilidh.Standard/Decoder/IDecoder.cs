using System.IO;
using ProjectCeilidh.Ceilidh.Standard.Audio;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    public interface IDecoder
    {
        bool TryDecode(Stream source, out IAudioData audioData);
    }
}
