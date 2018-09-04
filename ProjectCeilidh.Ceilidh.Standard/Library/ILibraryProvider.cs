namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    public interface ILibraryProvider
    {
        bool CanAccept(string uri);

        bool TryGetSource(string uri, out Source source);

        bool TryGetLibraryCollection(string uri, out ILibraryCollection sources);
    }
}
