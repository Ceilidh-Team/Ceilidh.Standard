using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Localization;

namespace ProjectCeilidh.Ceilidh.Standard.Filter
{
    [CobbleExport]
    public class EqualizerFilter : IFilterProvider
    {
        public string Name { get; }

        public EqualizerFilter(ILocalizationController localization)
        {
            Name = localization.Translate("filter.equalizer");
        }

        public AudioStream TransformAudioStream(AudioStream stream)
        {
            return null;
        }
    }
}
