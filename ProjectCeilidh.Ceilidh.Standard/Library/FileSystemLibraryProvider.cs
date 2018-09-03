using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

        public ISource GetSource(string uri) => new FileSystemSource(uri);

        #region ICollection

        public IEnumerator<string> GetEnumerator() => _monitorUris.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var add = false;

            _monitorUris.GetOrAdd(item, x =>
            {
                add = true;

                var watcher = new FileSystemWatcher()
                {
                    Path = x,
                    NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime |
                                   NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size
                };

                watcher.Created += WatcherOnCreated;
                watcher.Renamed += WatcherOnRenamed;
                watcher.Changed += WatcherOnChanged;
                watcher.Deleted += WatcherOnDeleted;
                
                watcher.EnableRaisingEvents = true;
                return watcher;
            });

            if (!add) return;

            UriChanged?.Invoke(this, new UriChangedEventArgs(UriChangedEventArgs.UriChangedAction.Added, item));

            foreach (var file in Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories))
                SourceChanged?.Invoke(this,
                    new SourceChangedEventArgs(item, file, SourceChangedEventArgs.SourceChangedAction.Added));

            UriChanged?.Invoke(this, new UriChangedEventArgs(UriChangedEventArgs.UriChangedAction.Ready, item));
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath)) return;

            var path = (sender as FileSystemWatcher)?.Path;

            SourceChanged?.Invoke(this, new SourceChangedEventArgs(path, e.FullPath, SourceChangedEventArgs.SourceChangedAction.Removed));
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath)) return;

            var path = (sender as FileSystemWatcher)?.Path;

            if (e.ChangeType == WatcherChangeTypes.Changed)
                SourceChanged?.Invoke(this,
                    new SourceChangedEventArgs(path, e.FullPath, SourceChangedEventArgs.SourceChangedAction.Updated));
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            var path = (sender as FileSystemWatcher)?.Path;

            if (path == null) throw new ArgumentException();

            if (Directory.Exists(e.FullPath))
            {
                foreach (var file in Directory.EnumerateFiles(e.FullPath, "*", SearchOption.AllDirectories))
                {
                    var chopPath = file.Substring(Path.GetFullPath(e.FullPath).Length);

                    SourceChanged?.Invoke(sender, new SourceChangedEventArgs(path, Path.Combine(e.OldFullPath, chopPath), SourceChangedEventArgs.SourceChangedAction.Removed));
                    SourceChanged?.Invoke(sender, new SourceChangedEventArgs(path, file, SourceChangedEventArgs.SourceChangedAction.Added));
                }
            }
            else
            {
                SourceChanged?.Invoke(sender, new SourceChangedEventArgs(path, e.OldFullPath, SourceChangedEventArgs.SourceChangedAction.Removed));
                SourceChanged?.Invoke(sender, new SourceChangedEventArgs(path, e.FullPath, SourceChangedEventArgs.SourceChangedAction.Added));
            }
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath)) return;

            var path = (sender as FileSystemWatcher)?.Path;

            SourceChanged?.Invoke(sender, new SourceChangedEventArgs(path, e.FullPath, SourceChangedEventArgs.SourceChangedAction.Added));
        }

        public void Clear()
        {
            foreach (var uri in _monitorUris.Keys)
                Remove(uri);
        }

        public bool Contains(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            return _monitorUris.ContainsKey(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _monitorUris.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (!_monitorUris.TryRemove(item, out var watcher)) return false;

            watcher.EnableRaisingEvents = false;

            watcher.Created -= WatcherOnCreated;
            watcher.Renamed -= WatcherOnRenamed;
            watcher.Changed -= WatcherOnChanged;
            watcher.Deleted -= WatcherOnDeleted;

            watcher.Dispose();

            UriChanged?.Invoke(this, new UriChangedEventArgs(UriChangedEventArgs.UriChangedAction.Removed, item));

            return true;

        }

        public int Count => _monitorUris.Count;

        public bool IsReadOnly => false;

        #endregion

        public event SourceChangedEventHandler SourceChanged;
        public event UriChangedEventHandler UriChanged;

        private struct FileSystemSource : ISource
        {
            public string Uri { get; }

            public FileSystemSource(string uri) => Uri = uri;

            public Stream GetStream() => File.OpenRead(Uri);
        }
    }
}
