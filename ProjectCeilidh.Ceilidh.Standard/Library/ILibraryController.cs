using System.Collections.Generic;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    public interface ILibraryController : ILateInject<ILibraryProvider>
    {
        /// <summary>
        ///     Try to produce a <see cref="ISource" /> for a given URI.
        /// </summary>
        /// <param name="uri">The URI to try and open.</param>
        /// <param name="source">The produced <see cref="ISource" /></param>
        /// <returns>True if the produced source is valid, false otherwise.</returns>
        bool TryGetSource(string uri, out ISource source);
    }
}
