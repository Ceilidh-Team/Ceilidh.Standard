using System;
using System.Collections.Generic;

namespace ProjectCeilidh.Ceilidh.Standard.Mount
{
    public interface IMountFolder : IMountRecord
    {
        IReadOnlyCollection<IMountRecord> Children { get; }

        bool TryCreateFile(Uri name, out IMountFile file);
        bool TryCreateFolder(Uri name, out IMountFolder folder);
    }
}