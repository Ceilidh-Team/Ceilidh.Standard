using System;
using System.Collections.ObjectModel;

namespace ProjectCeilidh.Ceilidh.Standard.Mount
{
    public interface IMountFolder : IMountRecord
    {
        ReadOnlyObservableCollection<IMountRecord> Children { get; }

        bool TryCreateFile(Uri name, out IMountFile file);
        bool TryCreateFolder(Uri name, out IMountFolder folder);
    }
}