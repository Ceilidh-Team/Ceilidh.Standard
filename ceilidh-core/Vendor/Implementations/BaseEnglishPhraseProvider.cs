using System.Collections.Generic;
using System.Globalization;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations
{
    public class BaseEnglishPhraseProvider : ILocalizationPhraseProvider
    {
        public IReadOnlyDictionary<string, string[]> GetPhrases(CultureInfo culture)
        {
            switch (culture)
            {
                case var c when c.TwoLetterISOLanguageName == "en" || c.Equals(CultureInfo.InvariantCulture):
                    return new Dictionary<string, string[]>
                    {
                        ["Hello"] = new[] { "Hello, {0}!" },
                        ["library.loaded"] = new[] { "Loaded {0} library provider", "Loaded {0} library providers" },
                        ["ffmpeg.disabled"] = new[] { "Failed to initialize FFmpeg" },
                        ["ffmpeg.util.version"] = new[] { "Loaded libavutil version {0}" },
                        ["ffmpeg.format.version"] = new[] { "Loaded libavformat version {0}" },
                        ["ffmpeg.codec.version"] = new[] { "Loaded libavcodec version {0}" },
                        ["ffmpeg.error.unsupported_codec"] = new []{ "FFmpeg does not support this codec" },
                        ["ffmpeg.error.unknown"] = new [] { "An unknown error has occurred within FFmpeg while executing \"{0}\"" },
                        ["ffmpeg.error.averror"] = new [] { "FFmpeg function \"{0}\" returned non-success error code {1}" }
                    };
                default: return null;
            }
        }
    }
}
