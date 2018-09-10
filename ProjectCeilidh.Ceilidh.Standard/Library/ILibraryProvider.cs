namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    public interface ILibraryProvider
    {
        /// <summary>
        /// Determines if this provider can accept the given URI based on its structure.
        /// </summary>
        /// <returns>True if the provider can accept this URI, false otherwise.</returns>
        /// <param name="uri">The URI to check.</param>
        bool CanAccept(string uri);

        /// <summary>
        /// Attempt to get a source for the given URI.
        /// </summary>
        /// <returns>True if the returned source is valid, false otherwise.</returns>
        /// <param name="uri">The URI to get a source for.</param>
        /// <param name="source">A source used for accessing the URI data.</param>
        bool TryGetSource(string uri, out Source source);

        /// <summary>
        /// Attempt to get a collection following changes and content in the given URI.
        /// </summary>
        /// <returns>True if a valid source was produced, false otherwise.</returns>
        /// <param name="uri">The URI to get a collection for.</param>
        /// <param name="sources">A collection used for accessing subordinate sources.</param>
        bool TryGetLibraryCollection(string uri, out ILibraryCollection sources);
    }
}
