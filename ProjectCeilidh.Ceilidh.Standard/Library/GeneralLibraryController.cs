using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProjectCeilidh.Ceilidh.Standard.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    [CobbleExport]
    public class GeneralLibraryController : ILibraryController
    {
        private readonly ConcurrentDictionary<string, ILibraryProvider> _libraryUris;
        private readonly ConcurrentBag<ILibraryProvider> _providers;

        public GeneralLibraryController(IEnumerable<ILibraryProvider> libraryProviders)
        {
            _libraryUris = new ConcurrentDictionary<string, ILibraryProvider>();
            _providers = new ConcurrentBag<ILibraryProvider>();

            foreach (var provider in libraryProviders)
                UnitLoaded(provider);
        }

        public void UnitLoaded(ILibraryProvider unit)
        {
            _providers.Add(unit);
        }

        public bool TryGetSource(string uri, out Source source)
        {
            source = default;
            var prov = _providers.FirstOrDefault(x => x.CanAccept(uri));
            if (prov == null) return false;
            return prov.TryGetSource(uri, out source);
        }

        public bool TryGetLibraryCollection(string uri, out ILibraryCollection sources)
        {
            sources = default;
            var prov = _providers.FirstOrDefault(x => x.CanAccept(uri));
            if (prov == null) return false;
            return prov.TryGetLibraryCollection(uri, out sources);
        }

        public IEnumerator<string> GetEnumerator() => _libraryUris.Keys.GetEnumerator();

        public void Clear()
        {
            _libraryUris.Clear();
        }

        public bool Contains(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            return _libraryUris.ContainsKey(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _libraryUris.Keys.CopyTo(array, arrayIndex);
        }

        public int Count => _libraryUris.Count;

        public bool IsReadOnly => false;
    }
}
