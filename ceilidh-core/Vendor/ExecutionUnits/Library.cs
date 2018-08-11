using System;
using System.Collections.Generic;
using System.Linq;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.ExecutionUnits
{
    public class Library : ILibraryController
    {
        private readonly ILocalizationController _localization;
        private readonly IReadOnlyList<ILibraryProvider> _libraryProviders;
        private readonly HashSet<string> _libraryUris = new HashSet<string>();

        public Library(IEnumerable<ILibraryProvider> libraryProviders, ILocalizationController localization)
        {
            _libraryProviders = new List<ILibraryProvider>(libraryProviders);
            _localization = localization;

            Console.WriteLine(_localization.Translate("library.loaded", _libraryProviders.Count));
        }

        public bool AddUri(string uri)
        {
            var prov = _libraryProviders.First(x => x.CanAccept(uri));

            if (prov == null || _libraryUris.Contains(uri)) return false;

            prov.AddUri(uri);
            _libraryUris.Add(uri);
            return true;
        }

        public bool RemoveUri(string uri)
        {
            var prov = _libraryProviders.First(x => x.CanAccept(uri));

            if (prov == null || _libraryUris.Contains(uri)) return false;

            prov.RemoveUri(uri);
            _libraryUris.Remove(uri);
            return true;
        }

        public bool TryGetTrack(string uri, out LowTrack track)
        {
            var prov = _libraryProviders.First(x => x.CanAccept(uri));

            track = prov?.GetTrack(uri);
            return track != null;
        }

        public IReadOnlyCollection<string> GetUris() => _libraryUris;
    }
}