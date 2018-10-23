using ProjectCeilidh.Ceilidh.Standard.Audio;
using ProjectCeilidh.Ceilidh.Standard.Library;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    public interface IDecoderController : ILateInject<IDecoder>
    {
        bool TryDecode(ISource source, out IAudioData audioData);
    }
}
