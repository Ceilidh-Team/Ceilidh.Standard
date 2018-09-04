using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    public interface ILibraryController : ILateInject<ILibraryProvider>
    {
        /// <summary>
        ///     Try to produce a <see cref="Source" /> for a given URI.
        /// </summary>
        /// <param name="uri">The URI to try and open.</param>
        /// <param name="source">The produced <see cref="Source" /></param>
        /// <returns>True if the produced source is valid, false otherwise.</returns>
        bool TryGetSource(string uri, out Source source);

        /// <summary>
        /// Try to produce an <see cref="ILibraryCollection"/> for a given URI.
        /// </summary>
        /// <returns><c>true</c>, if get some <see cref="ILibraryProvider"/> was able to produce a collection, <c>false</c> otherwise.</returns>
        /// <param name="uri">Library URI.</param>
        /// <param name="sources">Observable collection of sources</param>
        bool TryGetLibraryCollection(string uri, out ILibraryCollection sources);
    }
}
