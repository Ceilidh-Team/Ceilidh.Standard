using System.IO;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.Contracts
{
    /// <summary>
    ///     Arguments for the <see cref="ILibraryProvider.TrackAdded" /> event
    /// </summary>
    public struct TrackAddedEventArgs
    {
        /// <summary>
        ///     The library URI this event came from.
        /// </summary>
        public readonly string LibraryUri;

        /// <summary>
        ///     A LowTrack that can be used to access this track.
        /// </summary>
        public readonly LowTrack Track;

        public TrackAddedEventArgs(string libraryUri, LowTrack track)
        {
            LibraryUri = libraryUri;
            Track = track;
        }
    }

    /// <summary>
    ///     Arguments for the <see cref="ILibraryProvider.TrackRemoved" /> event
    /// </summary>
    public struct TrackRemovedEventArgs
    {
        /// <summary>
        ///     The library URI this event came from
        /// </summary>
        public readonly string LibraryUri;

        /// <summary>
        ///     The track URI that was removed
        /// </summary>
        public readonly string TrackUri;

        public TrackRemovedEventArgs(string libraryUri, string trackUri)
        {
            LibraryUri = libraryUri;
            TrackUri = trackUri;
        }
    }

    /// <summary>
    ///     Arguments for the <see cref="ILibraryProvider.UriChanged" /> event
    /// </summary>
    public struct UriChangedEventArgs
    {
        /// <summary>
        ///     The action done to the URI
        /// </summary>
        public readonly UriChangedAction Action;

        /// <summary>
        ///     The URI that changed
        /// </summary>
        public readonly string Uri;

        public UriChangedEventArgs(UriChangedAction action, string uri)
        {
            Action = action;
            Uri = uri;
        }
    }

    /// <summary>
    ///     The possible actions that could be done on a URI.
    /// </summary>
    public enum UriChangedAction
    {
        Added,
        Removed
    }

    /// <summary>
    ///     Arguments for the <see cref="ILibraryProvider.Ready" /> event
    /// </summary>
    public struct ReadyEventArgs
    {
        /// <summary>
        ///     The library URI that has finished enumeration
        /// </summary>
        public readonly string Uri;

        public ReadyEventArgs(string uri)
        {
            Uri = uri;
        }
    }

    /// <summary>
    ///     Event handler delegate for <see cref="ILibraryProvider.TrackAdded" />
    /// </summary>
    /// <param name="sender">The object that produced this event</param>
    /// <param name="e">The event arguments</param>
    public delegate void TrackAddedEventHandler(object sender, TrackAddedEventArgs e);

    /// <summary>
    ///     Event handler delegate for <see cref="ILibraryProvider.TrackRemoved" />
    /// </summary>
    /// <param name="sender">The object that produced this event</param>
    /// <param name="e">The event arguments</param>
    public delegate void TrackRemovedEventHandler(object sender, TrackRemovedEventArgs e);

    /// <summary>
    ///     Event handler delegate for <see cref="ILibraryProvider.UriChanged" />
    /// </summary>
    /// <param name="sender">The object that produced this event</param>
    /// <param name="e">The event arguments</param>
    public delegate void UriChangedEventHandler(object sender, UriChangedEventArgs e);

    /// <summary>
    ///     Event handler delegate for <see cref="ILibraryProvider.Ready" />
    /// </summary>
    /// <param name="sender">The object that produced this event</param>
    /// <param name="e">The event arguments</param>
    public delegate void ReadyEventHandler(object sender, ReadyEventArgs e);

    [Contract]
    public interface ILibraryProvider
    {
        /// <summary>
        ///     Determines if this library provider can accept the specified URI.
        /// </summary>
        /// <param name="uri">The URI to test.</param>
        /// <returns>True if this provider can accept the URI; False otherwise</returns>
        bool CanAccept(string uri);

        /// <summary>
        ///     Monitor a new URI
        /// </summary>
        /// <param name="uri">The URI to monitor</param>
        void AddUri(string uri);

        /// <summary>
        ///     Stop monitoring a URI
        /// </summary>
        /// <param name="uri">The URI to stop monitoring</param>
        void RemoveUri(string uri);

        /// <summary>
        ///     Get a <see cref="LowTrack" /> instance for the specified URI
        /// </summary>
        /// <param name="uri">The track URI</param>
        /// <returns>A <see cref="LowTrack" /> instance that provides access to the specified track</returns>
        LowTrack GetTrack(string uri);

        /// <summary>
        ///     Emitted when a new track is detected through monitoring.
        /// </summary>
        event TrackAddedEventHandler TrackAdded;

        /// <summary>
        ///     Emitted when a monitored track is deleted.
        /// </summary>
        event TrackRemovedEventHandler TrackRemoved;

        /// <summary>
        ///     Emitted whenever a change to the set of URIs to monitor occurs.
        /// </summary>
        event UriChangedEventHandler UriChanged;

        /// <summary>
        ///     Emitted when the initial track listing is complete for a new URI.
        /// </summary>
        event ReadyEventHandler Ready;
    }

    /// <summary>
    ///     A container object that describes how to access track data at a low level.
    /// </summary>
    public abstract class LowTrack
    {
        /// <summary>
        ///     The URI for this track.
        /// </summary>
        public string Uri { get; protected set; }

        /// <summary>
        ///     Get a stream for the specified track.
        /// </summary>
        /// <returns>A readable stream containing track data.</returns>
        public abstract Stream GetStream();
    }
}