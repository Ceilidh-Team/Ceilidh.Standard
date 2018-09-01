using System.Collections.Generic;
using System.Globalization;

namespace ProjectCeilidh.Ceilidh.Standard.Localization
{
    /// <summary>
    /// Provides phrases to the localization controller
    /// </summary>
    public interface ILocalizationPhraseProvider
    {
        IReadOnlyDictionary<string, string[]> GetPhrases(CultureInfo culture);
    }
}
