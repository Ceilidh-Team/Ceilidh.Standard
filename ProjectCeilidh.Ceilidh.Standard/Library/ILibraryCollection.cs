using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    public interface ILibraryCollection : IReadOnlyCollection<Source>, INotifyCollectionChanged, IDisposable
    {
        string Uri { get; }
    }

    /* public class LibraryCollection : ICollection<Source>, ILibraryCollection
    {
        public int Count => _sources.Count;

        public bool IsReadOnly => true;

        private readonly HashSet<Source> _sources;

        public LibraryCollection()
        {
            _sources = new HashSet<Source>();
        }

        public void Add(Source item)
        {
            if (!_sources.Add(item)) return;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            _sources.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(Source item) => _sources.Contains(item);

        public void CopyTo(Source[] array, int arrayIndex) => _sources.CopyTo(array, arrayIndex);

        public IEnumerator<Source> GetEnumerator() => _sources.GetEnumerator();

        public bool Remove(Source item)
        {
            if (!_sources.Remove(item)) return false;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }*/
}
