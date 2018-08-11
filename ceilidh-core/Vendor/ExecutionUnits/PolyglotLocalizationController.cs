using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.ExecutionUnits
{
    public class PolyglotLocalizationController : ILocalizationController
    {
        /// <summary>
        /// Specifies a plural group for each supported culture.
        /// </summary>
        private static readonly IReadOnlyDictionary<CultureInfo, PluralGroups> CultureGroups = new Dictionary<PluralGroups, string[]>
        {
            [PluralGroups.Arabic] = new[] { "ar" },
            [PluralGroups.BosnianSerbian] = new[] { "bs-Latn-BA", "bs-Cyrl-BA", "srl-RS", "sr-RS" },
            [PluralGroups.Chinese] = new[] { "id", "id-ID", "ja", "ko", "ko-KR", "lo", "ms", "th", "th-TH", "zh" },
            [PluralGroups.Croatian] = new[] { "hr", "hr-HR" },
            [PluralGroups.German] = new[] { "", "fa", "da", "de", "en", "es", "fi", "el", "he", "hi-IN", "hu", "hu-HU", "it", "nl", "no", "pt", "sv", "tr" }, // Note: The invariant culture is English, but not region-specific.
            [PluralGroups.French] = new[] { "fr", "tl", "pt-br" },
            [PluralGroups.Russian] = new[] { "ru", "ru-RU" },
            [PluralGroups.Lithuanian] = new[] { "lt" },
            [PluralGroups.Czech] = new[] { "cs", "cs-CZ", "sk" },
            [PluralGroups.Polish] = new[] { "pl" },
            [PluralGroups.Icelandic] = new[] { "is" },
            [PluralGroups.Slovenian] = new[] { "sl-SL" },
        }.SelectMany(x => x.Value.Select(y => (culture: CultureInfo.GetCultureInfo(y), pluralGroup: x.Key))).ToDictionary(x => x.culture, y => y.pluralGroup);

        /// <summary>
        /// The culture used for pluralization operations.
        /// </summary>
        public CultureInfo Culture { get; set; }

        private readonly Dictionary<string, string[]> _phrases;

        public PolyglotLocalizationController(IEnumerable<ILocalizationPhraseProvider> phraseProviders)
        {
            Culture = CultureInfo.CurrentUICulture;
            _phrases = new Dictionary<string, string[]>();

            foreach (var provider in phraseProviders)
            {
                var phrases = provider.GetPhrases(Culture);
                if (phrases != null) Extend(phrases);
            }
        }

        /// <summary>
        /// Clear the phrase bank.
        /// </summary>
        public void Clear()
        {
            _phrases.Clear();
        }

        /// <summary>
        /// Extend the phrase bank with new phrases.
        /// </summary>
        /// <param name="phrases">The new phrases to add.</param>
        public void Extend(IReadOnlyDictionary<string, string[]> phrases)
        {
            foreach (var (key, phrase) in phrases)
            {
                if (phrase == null || phrase.Length <= 0) throw new ArgumentException($"Phrase for key \"{key}\" was null or had no data.", nameof(phrases));

                _phrases[key] = phrase;
            }
        }

        /// <summary>
        /// Replace the phrase bank with new phrases.
        /// </summary>
        /// <param name="phrases">The new phrase bank.</param>
        public void Replace(IReadOnlyDictionary<string, string[]> phrases)
        {
            Clear();
            Extend(phrases);
        }

        /// <summary>
        /// Translate the given key with the specified interpolation arguments.
        /// </summary>
        /// <param name="key">The key to translate</param>
        /// <param name="args">The arguments to pass to <see cref="string.Format(string,object[])"/>.</param>
        /// <returns></returns>
        public string Translate(string key, params object[] args)
        {
            if (!_phrases.TryGetValue(key, out var phrase)) return key;

            string specific;

            if (args.Length > 0 && IsIntegral(args[0], out var count))
            { // Apply smart-pluralization
                var idx = PluralPhraseIndex(Culture, count);
                specific = idx >= phrase.Length ? phrase[0] : phrase[idx];
            }
            else
                specific = phrase[0];

            try
            {
                return string.Format(specific, args);
            }
            catch (FormatException)
            {
                return specific;
            }
        }

        /// <summary>
        /// Determine if the value is an integer, and convert to decimal if it is.
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <param name="num">The produced value</param>
        /// <returns>True if the value was integral and a good value is produced, false otherwise</returns>
        private static bool IsIntegral(object value, out decimal num)
        {
            num = default(decimal);
            if (value == null) return false;

            var typ = value.GetType();

            var nType = Nullable.GetUnderlyingType(typ);
            if (nType != null) typ = nType;

            switch (Type.GetTypeCode(typ))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    num = Convert.ToDecimal(value);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// For a given count, determine the specific string that should contain the properly pluralized version.
        /// </summary>
        /// <param name="culture">The culture used to decide the pluralization strategy</param>
        /// <param name="count">The number used to determine pluralization.</param>
        /// <returns>The index into the phrase array to use when looking for </returns>
        private static int PluralPhraseIndex(CultureInfo culture, decimal count)
        {
            if (!CultureGroups.TryGetValue(culture, out var group) && !CultureGroups.TryGetValue(CultureInfo.GetCultureInfo(culture.TwoLetterISOLanguageName), out group))
                group = CultureGroups[CultureInfo.InvariantCulture];

            var lastTwo = count % 100;
            var end = count % 10;

            switch (group)
            {
                case PluralGroups.Arabic:
                    if (count < 3) return (int)count;
                    if (lastTwo >= 3 && lastTwo <= 10) return 3;
                    return lastTwo >= 11 ? 4 : 5;
                case PluralGroups.BosnianSerbian:
                case PluralGroups.Croatian:
                case PluralGroups.Russian:
                    if (lastTwo != 11 && end == 1)
                        return 0;

                    if (2 <= end && end <= 4 && !(lastTwo >= 12 && lastTwo <= 14))
                        return 1;

                    return 2;
                case PluralGroups.Chinese:
                    return 0;
                case PluralGroups.French:
                    return count > 1 ? 1 : 0;
                case PluralGroups.German:
                    return count != 1 ? 1 : 0;
                case PluralGroups.Lithuanian:
                    if (end == 1 && lastTwo != 11) return 0;
                    return end >= 2 && end <= 9 && (lastTwo < 11 || lastTwo > 19) ? 1 : 2;
                case PluralGroups.Czech:
                    if (count == 1) return 0;
                    return count >= 2 && count <= 4 ? 1 : 2;
                case PluralGroups.Polish:
                    if (count == 1) return 0;
                    return 2 <= count && end <= 4 && (lastTwo < 10 || lastTwo >= 20) ? 1 : 2;
                case PluralGroups.Icelandic:
                    return end != 1 || lastTwo == 11 ? 1 : 0;
                case PluralGroups.Slovenian:
                    switch (lastTwo)
                    {
                        case 1:
                            return 0;
                        case 2:
                            return 1;
                        case 3:
                        case 4:
                            return 2;
                        default:
                            return 3;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum PluralGroups
        {
            Arabic,
            BosnianSerbian,
            Chinese,
            Croatian,
            French,
            German,
            Russian,
            Lithuanian,
            Czech,
            Polish,
            Icelandic,
            Slovenian
        }
    }
}
