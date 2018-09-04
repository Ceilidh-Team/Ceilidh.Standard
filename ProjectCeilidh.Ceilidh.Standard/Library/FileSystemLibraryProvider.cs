using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using ProjectCeilidh.Ceilidh.Standard.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    [CobbleExport]
    public class FileSystemLibraryProvider : ILibraryProvider
    {
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _monitorUris;

        public FileSystemLibraryProvider()
        {
            _monitorUris = new ConcurrentDictionary<string, FileSystemWatcher>();
        }

        public bool CanAccept(string uri)
        {
            try
            {
                return Path.IsPathRooted(uri);
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetSource(string uri, out Source source)
        {
            source = default;
            if (!Path.IsPathRooted(uri) || !File.Exists(uri)) return false;

            source = new FileSystemSource(uri);
            return true;
        }

        public bool TryGetLibraryCollection(string uri, out ILibraryCollection sources)
        {
            sources = default;
            if (!Path.IsPathRooted(uri) || !Directory.Exists(uri)) return false;

            sources = new FileSystemLibraryCollection(uri);
            return true;
        }

        private class FileSystemSource : Source
        {
            public FileSystemSource(string uri) => Uri = uri;

            public override Stream GetStream() => File.OpenRead(Uri);
        }

        private class FileSystemLibraryCollection : ILibraryCollection
        {
            public string Uri { get; }

            public int Count => throw new NotImplementedException();

            private readonly ConcurrentDictionary<string, Source> _files;
            private readonly FileSystemWatcher _watcher;

            public FileSystemLibraryCollection(string uri)
            {
                Uri = uri;

                _files = new ConcurrentDictionary<string, Source>(Directory.EnumerateFiles(uri, "*", SearchOption.AllDirectories).Select(x => new KeyValuePair<string, Source>(x, new FileSystemSource(x))));
                _watcher = new FileSystemWatcher
                {
                    Path = uri,
                    NotifyFilter = NotifyFilters.Attributes
                                                | NotifyFilters.CreationTime
                                                | NotifyFilters.DirectoryName
                                                | NotifyFilters.FileName
                                                | NotifyFilters.LastAccess
                                                | NotifyFilters.LastWrite
                                                | NotifyFilters.Security
                                                | NotifyFilters.Size,
                };

                _watcher.Changed += WatcherOnChanged;
                _watcher.Created += WatcherOnCreated;
                _watcher.Deleted += WatcherOnDeleted;
                _watcher.Renamed += WatcherOnRenamed;

                _watcher.EnableRaisingEvents = true;
            }

            void WatcherOnChanged(object sender, FileSystemEventArgs e)
            {
                if (e.ChangeType != WatcherChangeTypes.Changed) return;

                var src = new FileSystemSource(e.FullPath);

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, src, src));
            }

            void WatcherOnCreated(object sender, FileSystemEventArgs e)
            {
                var src = new FileSystemSource(e.FullPath);

                if (!_files.TryAdd(e.FullPath, src)) return;

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, src));
            }

            void WatcherOnDeleted(object sender, FileSystemEventArgs e)
            {
                if (!_files.TryRemove(e.FullPath, out var src)) return;

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, src));
            }

            void WatcherOnRenamed(object sender, RenamedEventArgs e)
            {
                if (_files.TryRemove(e.OldFullPath, out var oldSrc))
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldSrc));

                var newSrc = new FileSystemSource(e.FullPath);

                if (_files.TryAdd(e.FullPath, newSrc))
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newSrc));
            }

            public IEnumerator<Source> GetEnumerator() => _files.Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Dispose()
            {
                _watcher.EnableRaisingEvents = false;

                _watcher.Changed -= WatcherOnChanged;
                _watcher.Created -= WatcherOnCreated;
                _watcher.Deleted -= WatcherOnDeleted;
                _watcher.Renamed -= WatcherOnRenamed;
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
        }
    }
}
