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
            unit.UriChanged += UnitOnUriChanged;
            unit.SourceChanged += UnitOnSourceChanged;

            _providers.Add(unit);
        }

        private void UnitOnSourceChanged(object sender, SourceChangedEventArgs e)
        {

        }

        private void UnitOnUriChanged(object sender, UriChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool TryGetSource(string uri, out ISource source) => (source = _providers.FirstOrDefault(x => x.CanAccept(uri))?.GetSource(uri)) != null;

        public IEnumerator<string> GetEnumerator() => _libraryUris.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

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
            if (item == null) throw new ArgumentNullException(nameof(item));

            return _libraryUris.ContainsKey(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _libraryUris.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            return _libraryUris.TryRemove(item, out var prov) && prov.Remove(item);
        }

        public int Count => _libraryUris.Count;

        public bool IsReadOnly => false;
    }
}
