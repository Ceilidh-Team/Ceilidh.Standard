using System.Collections.Generic;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Filter.FFmpeg;
using ProjectCeilidh.Ceilidh.Standard.Localization;

namespace ProjectCeilidh.Ceilidh.Standard.Filter
{
    [CobbleExport]
    public class ReformatFilter : IFilterProvider
    {
        public string Name { get; }

        public ReformatFilter(ILocalizationController localization)
        {
            Name = localization.Translate("filter.resample");
        }

        public AudioStream TransformAudioStream(AudioStream stream)
        {
            return new FFmpegFilterAudioStream(stream, new AudioFormat(192000, 2, stream.Format.DataFormat), new FilterConfiguration("aformat", new Dictionary<string, string>
            {
                //["sample_fmts"] = "s32",
                ["sample_rates"] = "192000",
                ["channel_layouts"] = "2c"
            })); // new AudioDataFormat(NumberFormat.Signed, !BitConverter.IsLittleEndian, 4)));
        }
    }
}
