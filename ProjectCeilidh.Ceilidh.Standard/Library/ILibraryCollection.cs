using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    public interface ILibraryCollection : IReadOnlyCollection<Source>, INotifyCollectionChanged, IDisposable
    {
        string Uri { get; }
    }
}
