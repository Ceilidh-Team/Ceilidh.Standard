using System.Collections.Generic;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Localization
{
    /// <summary>
    /// Handles unified localization tasks
    /// </summary>
    public interface ILocalizationController : ILateInject<ILocalizationPhraseProvider>
    {
        void Extend(IReadOnlyDictionary<string, string[]> phrases);
        string Translate(string key, params object[] args);
    }
}
