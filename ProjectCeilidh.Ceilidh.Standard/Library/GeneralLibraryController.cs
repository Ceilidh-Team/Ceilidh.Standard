using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _providers = new ConcurrentBag<ILibraryProvider>(libraryProviders);
        }

        public void UnitLoaded(ILibraryProvider unit) => _providers.Add(unit);

        public bool TryGetSource(string uri, out ISource source) => (source = _providers.FirstOrDefault(x => x.CanAccept(uri))?.GetSource(uri)) != null;

        public IEnumerator<string> GetEnumerator() => _libraryUris.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(string item)
        {
            var prov = _providers.FirstOrDefault(x => x.CanAccept(item));
            if (prov == null) return;

            _libraryUris.TryAdd(item, prov);

            prov.Add(item); // TODO: Events
        }

        public void Clear()
        {
            _libraryUris.Clear();
        }

        public bool Contains(string item)
        {
            return _libraryUris.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _libraryUris.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return _libraryUris.Remove(item);
        }

        public int Count => _libraryUris.Count;

        public bool IsReadOnly => false;
    }
}
