using System.Collections.Generic;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    public delegate void SourceChangedEventHandler(object sender, SourceChangedEventArgs e);

    public delegate void UriChangedEventHandler(object sender, UriChangedEventArgs e);

    public interface ILibraryProvider : ICollection<string>
    {
        bool CanAccept(string uri);

        ISource GetSource(string uri);

        event SourceChangedEventHandler SourceChanged;
        event UriChangedEventHandler UriChanged;
    }

    public struct SourceChangedEventArgs
    {
        public readonly string LibraryUri;
        public readonly string SourceUri;
        public readonly SourceChangedAction Action;

        public SourceChangedEventArgs(string libraryUri, string sourceUri, SourceChangedAction action)
        {
            LibraryUri = libraryUri;
            SourceUri = sourceUri;
            Action = action;
        }

        public enum SourceChangedAction
        {
            Added,
            Removed,
            Updated
        }
    }

    public struct UriChangedEventArgs
    {
        public readonly UriChangedAction Action;
        public readonly string Uri;

        public UriChangedEventArgs(UriChangedAction action, string uri)
        {
            Action = action;
            Uri = uri;
        }

        public enum UriChangedAction
        {
            Added,
            Removed,
            Ready
        }
    }
}
