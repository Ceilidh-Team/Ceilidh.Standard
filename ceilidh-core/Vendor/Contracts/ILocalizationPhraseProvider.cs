using System.Collections.Generic;
using System.Globalization;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.Contracts
{
    [Contract]
    public interface ILocalizationPhraseProvider
    {
        IReadOnlyDictionary<string, string[]> GetPhrases(CultureInfo culture);
    }
}
