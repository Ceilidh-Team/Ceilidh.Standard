using ProjectCeilidh.Ceilidh.Standard.Audio;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Filter
{
    public interface IFilterProvider
    {
        string Name { get; }

        AudioStream TransformAudioStream(AudioStream stream);
    } 
}
