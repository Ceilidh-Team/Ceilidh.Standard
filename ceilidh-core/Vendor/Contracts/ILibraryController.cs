using System.Collections.Generic;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.Contracts
{
    [Contract(Singleton = true)]
    public interface ILibraryController
    {
        /// <summary>
        ///     Attempt to add a new URI to the library.
        ///     This will begin monitoring it for changes.
        /// </summary>
        /// <param name="uri">The URI to begin monitoring</param>
        /// <returns>True if the URI could be handled and was added, false otherwise</returns>
        bool AddUri(string uri);

        /// <summary>
        ///     Attempt to remove a URI from the library.
        ///     This will end monitoring.
        /// </summary>
        /// <param name="uri">The URI to stop monitoring.</param>
        /// <returns>True if the URI could be handled and was removed, false otherwise.</returns>
        bool RemoveUri(string uri);

        /// <summary>
        ///     Try to produce a <see cref="LowTrack" /> for a given URI.
        /// </summary>
        /// <param name="uri">The URI to try and open.</param>
        /// <param name="track">The produced <see cref="LowTrack" /></param>
        /// <returns>True if the produced track is valid, false otherwise.</returns>
        bool TryGetTrack(string uri, out LowTrack track);

        /// <summary>
        /// Get all monitored library URIs.
        /// </summary>
        /// <returns>A collection of library URIs</returns>
        IReadOnlyCollection<string> GetUris();
    }
}