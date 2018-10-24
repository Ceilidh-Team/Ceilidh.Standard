using System;

namespace ProjectCeilidh.Ceilidh.Standard.Mount
{
    public interface IMountDriver
    {
        bool CanAccept(Uri uri);
        bool TryGetRecord(Uri uri, out IMountRecord record);
    }
}