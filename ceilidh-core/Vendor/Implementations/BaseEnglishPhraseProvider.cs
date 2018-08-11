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
                case var c when c.TwoLetterISOLanguageName == "en" || Equals(c, CultureInfo.InvariantCulture):
                    return new Dictionary<string, string[]>
                    {
                        ["Hello"] = new[] { "Hello, {0}!" },
                        ["library.loaded"] = new[] { "Loaded {0} library provider", "Loaded {0} library providers" }
                    };
                default: return null;
            }
        }
    }
}
