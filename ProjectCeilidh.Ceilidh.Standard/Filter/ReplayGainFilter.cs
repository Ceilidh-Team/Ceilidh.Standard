using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Filter.FFmpeg;

namespace ProjectCeilidh.Ceilidh.Standard.Filter
{
    [CobbleExport]
    public class ReplayGainFilter : IFilterProvider
    {
        private const string DB_REGEX = @"(?<value>-?\d+(?:\.\d+)?) dB";

        public string Name => "ReplayGain";

        public AudioStream TransformAudioStream(AudioStream stream)
        {
            var db = new Decibel(0);

            var albumGainString = stream.ParentData.Metadata.FirstOrDefault(x =>
                x.Key.Equals("REPLAYGAIN_ALBUM_GAIN", StringComparison.InvariantCultureIgnoreCase)).Value;
            var trackGainString = stream.ParentData.Metadata.FirstOrDefault(x =>
                x.Key.Equals("REPLAYGAIN_TRACK_GAIN", StringComparison.InvariantCultureIgnoreCase)).Value;

            if (albumGainString != null)
                db = new Decibel(double.Parse(Regex.Match(albumGainString, DB_REGEX).Groups["value"].Value));
            if (trackGainString != null)
                db = new Decibel(double.Parse(Regex.Match(trackGainString, DB_REGEX).Groups["value"].Value));

            var ratio = db.GetAmplitudeRatio();

            return new FFmpegFilterAudioStream(stream,
                new AudioFormat(stream.Format.SampleRate, stream.Format.Channels,
                    new AudioDataFormat(NumberFormat.FloatingPoint, !BitConverter.IsLittleEndian, 4)),
                new FilterConfiguration("volume", new Dictionary<string, string>
                {
                    ["volume"] = ratio.ToString(CultureInfo.InvariantCulture)
                }));
        }

        
    }
}
