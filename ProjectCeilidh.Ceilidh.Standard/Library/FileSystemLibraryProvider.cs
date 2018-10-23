﻿using System.Collections;
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

        public bool TryGetSource(string uri, out ISource source)
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

        private class FileSystemSource : ISource
        {
            public string Uri { get; }

            public FileSystemSource(string uri) => Uri = uri;

            public Stream GetStream() => File.OpenRead(Uri);
        }

        private class FileSystemLibraryCollection : ILibraryCollection
        {
            public string Uri { get; }

            public int Count => _files.Count;

            private readonly ConcurrentDictionary<string, ISource> _files;
            private readonly FileSystemWatcher _watcher;

            public FileSystemLibraryCollection(string uri)
            {
                Uri = uri;

                _files = new ConcurrentDictionary<string, ISource>(Directory
                    .EnumerateFiles(uri, "*", SearchOption.AllDirectories)
                    .Select(x => new KeyValuePair<string, ISource>(x, new FileSystemSource(x))));
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

            private void WatcherOnChanged(object sender, FileSystemEventArgs e)
            {
                if (e.ChangeType != WatcherChangeTypes.Changed) return;

                var src = _files.GetOrAdd(e.FullPath, x => new FileSystemSource(x));

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, src, src));
            }

            private void WatcherOnCreated(object sender, FileSystemEventArgs e)
            {
                var src = new FileSystemSource(e.FullPath);

                if (!_files.TryAdd(e.FullPath, src)) return;

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, src));
            }

            private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
            {
                if (!_files.TryRemove(e.FullPath, out var src)) return;

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, src));
            }

            private void WatcherOnRenamed(object sender, RenamedEventArgs e)
            {
                if (_files.TryRemove(e.OldFullPath, out var oldSrc))
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldSrc));

                var newSrc = new FileSystemSource(e.FullPath);

                if (_files.TryAdd(e.FullPath, newSrc))
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newSrc));
            }

            public IEnumerator<ISource> GetEnumerator() => _files.Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Dispose()
            {
                _watcher.EnableRaisingEvents = false;

                _watcher.Changed -= WatcherOnChanged;
                _watcher.Created -= WatcherOnCreated;
                _watcher.Deleted -= WatcherOnDeleted;
                _watcher.Renamed -= WatcherOnRenamed;

                _watcher.Dispose();
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
        }
    }
}
