using System;

namespace ProjectCeilidh.Ceilidh.Standard.Mount
{
    public interface IMountDriver
    {
        bool CanAccept(Uri uri);
        bool TryGetRecord(Uri uri, out IMountRecord record);
        bool TryWatchFolder(Uri uri, out IMountWatch watch);
    }
}