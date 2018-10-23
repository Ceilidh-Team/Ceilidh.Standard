using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProjectCeilidh.Ceilidh.Standard.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    [CobbleExport]
    public class GeneralLibraryController : ILibraryController
    {
        private readonly ConcurrentBag<ILibraryProvider> _providers;

        public GeneralLibraryController(IEnumerable<ILibraryProvider> libraryProviders)
        {
            _providers = new ConcurrentBag<ILibraryProvider>();

            foreach (var provider in libraryProviders)
                UnitLoaded(provider);
        }

        public void UnitLoaded(ILibraryProvider unit)
        {
            _providers.Add(unit);
        }

        public bool TryGetSource(string uri, out ISource source)
        {
            source = default;
            var prov = _providers.FirstOrDefault(x => x.CanAccept(uri));
            return prov != null && prov.TryGetSource(uri, out source);
        }

        public bool TryGetLibraryCollection(string uri, out ILibraryCollection sources)
        {
            sources = default;
            var prov = _providers.FirstOrDefault(x => x.CanAccept(uri));
            return prov != null && prov.TryGetLibraryCollection(uri, out sources);
        }
    }
}
