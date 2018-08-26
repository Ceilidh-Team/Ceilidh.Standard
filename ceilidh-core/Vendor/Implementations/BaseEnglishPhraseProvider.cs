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
                        ["libav.disabled"] = new[] { "Failed to initialize FFmpeg" },
                        ["libav.util.version"] = new[] { "Loaded libavutil version {0}" },
                        ["libav.format.version"] = new[] { "Loaded libavformat version {0}" },
                        ["libav.codec.version"] = new[] { "Loaded libavcodec version {0}" }
                    };
                default: return null;
            }
        }
    }
}
