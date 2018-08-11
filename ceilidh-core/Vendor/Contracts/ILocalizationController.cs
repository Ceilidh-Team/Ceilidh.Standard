using System.Collections.Generic;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.Contracts
{
    [Contract(Singleton = true)]
    public interface ILocalizationController
    {
        void Extend(IReadOnlyDictionary<string, string[]> phrases);
        string Translate(string key, params object[] args);
    }
}
