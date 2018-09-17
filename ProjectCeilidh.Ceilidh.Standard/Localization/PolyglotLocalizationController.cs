using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using ProjectCeilidh.Ceilidh.Standard.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Localization
{
    /// <summary>
    /// Uses algorithms from AirBnb's Polyglot library to provide localization.
    /// </summary>
    [CobbleExport]
    public class PolyglotLocalizationController : ILocalizationController
    {
        public const string POLYGLOT_PHRASE_ERROR = "polyglot.phraseError";

        private static readonly IReadOnlyDictionary<CultureInfo, PluralGroup> CultureGroups = new Dictionary<PluralGroup, string[]>
        {
            [PluralGroup.Arabic] = new[] { "ar" },
            [PluralGroup.BosnianSerbian] = new[] { "bs-Latn-BA", "bs-Cyrl-BA", "sr-Cyrl-RS", "sr-Latin-RS" },
            [PluralGroup.Chinese] = new[] { "id", "id-ID", "ja", "ko", "ko-KR", "lo", "ms", "th", "th-TH", "zh" },
            [PluralGroup.Croatian] = new[] { "hr", "hr-HR" },
            [PluralGroup.German] = new[] { "", "fa", "da", "de", "en", "es", "fi", "el", "he", "hi-IN", "hu", "hu-HU", "it", "nl", "no", "pt", "sv", "tr" }, // Note: The invariant culture is English, but not region-specific.
            [PluralGroup.French] = new[] { "fr", "fil", "pt-br" },
            [PluralGroup.Russian] = new[] { "ru", "ru-RU" },
            [PluralGroup.Lithuanian] = new[] { "lt" },
            [PluralGroup.Czech] = new[] { "cs", "cs-CZ", "sk" },
            [PluralGroup.Polish] = new[] { "pl" },
            [PluralGroup.Icelandic] = new[] { "is" },
            [PluralGroup.Slovenian] = new[] { "sl-SI" }
        }.SelectMany(x => x.Value.Select(y =>
        {
            try
            {
                return (Culture: CultureInfo.GetCultureInfo(y), PluralGroup: x.Key);
            }
            catch
            {
                Debug.WriteLine($"Failed to load a culture for locale \"{y}\"");
                return (null, x.Key);
            }
        }).Where(y => y.Culture != null)).ToDictionary(x => x.Culture, y => y.PluralGroup);

        /// <summary>
        /// The culture used for pluralization options.
        /// </summary>
        public CultureInfo Culture { get; set; }

        private readonly Dictionary<string, string[]> _phrases;

        public PolyglotLocalizationController(IEnumerable<ILocalizationPhraseProvider> phraseProviders)
        {
            Culture = CultureInfo.CurrentUICulture;
            _phrases = new Dictionary<string, string[]>();

            foreach (var provider in phraseProviders)
                Extend(provider.GetPhrases(Culture));
        }

        public void Extend(IReadOnlyDictionary<string, string[]> phrases)
        {
            if (phrases == null) return;

            foreach (var (key, phrase) in phrases)
            {
                if (phrase == null || phrase.Length <= 0) throw new ArgumentException(Translate(POLYGLOT_PHRASE_ERROR, key), nameof(phrases));

                _phrases[key] = phrase;
            }
        }

        public string Translate(string key, params object[] args)
        {
            if (!_phrases.TryGetValue(key, out var phrase)) return key;

            string specific;

            if (args.Length > 0 && IsIntegral(args[0], out var count)) // Apply smart-pluralization
            {
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

        public void UnitLoaded(ILocalizationPhraseProvider unit)
        {
            Extend(unit.GetPhrases(Culture));
        }

        /// <summary>
        /// Determine if the value is an integer, and convert to decimal if it is.
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <param name="num">The produced value</param>
        /// <returns>True if the value was integral and a good value is produced, false otherwise</returns>
        private static bool IsIntegral(object value, out decimal num)
        {
            num = default;
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
                case PluralGroup.Arabic:
                    if (count < 3) return (int)count;
                    if (lastTwo >= 3 && lastTwo <= 10) return 3;
                    return lastTwo >= 11 ? 4 : 5;
                case PluralGroup.BosnianSerbian:
                case PluralGroup.Croatian:
                case PluralGroup.Russian:
                    if (lastTwo != 11 && end == 1)
                        return 0;

                    if (2 <= end && end <= 4 && !(lastTwo >= 12 && lastTwo <= 14))
                        return 1;

                    return 2;
                case PluralGroup.Chinese:
                    return 0;
                case PluralGroup.French:
                    return count > 1 ? 1 : 0;
                case PluralGroup.German:
                    return count != 1 ? 1 : 0;
                case PluralGroup.Lithuanian:
                    if (end == 1 && lastTwo != 11) return 0;
                    return end >= 2 && end <= 9 && (lastTwo < 11 || lastTwo > 19) ? 1 : 2;
                case PluralGroup.Czech:
                    if (count == 1) return 0;
                    return count >= 2 && count <= 4 ? 1 : 2;
                case PluralGroup.Polish:
                    if (count == 1) return 0;
                    return 2 <= count && end <= 4 && (lastTwo < 10 || lastTwo >= 20) ? 1 : 2;
                case PluralGroup.Icelandic:
                    return end != 1 || lastTwo == 11 ? 1 : 0;
                case PluralGroup.Slovenian:
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

        private enum PluralGroup
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
