using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations
{
    /// <summary>
    ///     Provides library services for the filesystem
    /// </summary>
    public class FilesystemLibraryProvider : ILibraryProvider
    {
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _uriList =
            new ConcurrentDictionary<string, FileSystemWatcher>();

        public bool CanAccept(string uri) => Path.IsPathRooted(uri);

        public async void AddUri(string uri)
        {
            uri = Path.GetFullPath(uri);

            if (!Directory.Exists(uri)) return;

            var watch = new FileSystemWatcher();
            if (!_uriList.TryAdd(uri, watch)) return;

            UriChanged?.Invoke(this, new UriChangedEventArgs(UriChangedAction.Added, uri));

            watch.IncludeSubdirectories = true;
            watch.Path = uri;
            watch.NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.LastWrite;

            watch.Changed += WatchOnEvent;
            watch.Created += WatchOnEvent;
            watch.Deleted += WatchOnEvent;
            watch.Renamed += WatchOnRename;

            watch.EnableRaisingEvents = true;

            await Task.Run(() =>
            {
                foreach (var file in Directory.EnumerateFiles(uri, "*", SearchOption.AllDirectories))
                    TrackAdded?.Invoke(this, new TrackAddedEventArgs(uri, new FilesystemLowTrack(file)));
                Ready?.Invoke(this, new ReadyEventArgs(uri));
            });
        }

        public void RemoveUri(string uri)
        {
            uri = Path.GetFullPath(uri);

            if (!_uriList.TryRemove(uri, out var watch)) return;

            UriChanged?.Invoke(this, new UriChangedEventArgs(UriChangedAction.Removed, uri));

            watch.EnableRaisingEvents = false;

            watch.Changed -= WatchOnEvent;
            watch.Created -= WatchOnEvent;
            watch.Deleted -= WatchOnEvent;
            watch.Renamed -= WatchOnRename;
            watch.Dispose();
        }

        public LowTrack GetTrack(string uri) => new FilesystemLowTrack(uri);

        public event TrackAddedEventHandler TrackAdded;
        public event TrackRemovedEventHandler TrackRemoved;
        public event UriChangedEventHandler UriChanged;
        public event ReadyEventHandler Ready;

        private void WatchOnEvent(object sender, FileSystemEventArgs e)
        {
            var watch = (FileSystemWatcher) sender;

            if (Directory.Exists(e.FullPath)) return;

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    TrackRemoved?.Invoke(this, new TrackRemovedEventArgs(watch.Path, e.FullPath));
                    TrackAdded?.Invoke(this, new TrackAddedEventArgs(watch.Path, GetTrack(e.FullPath)));
                    break;
                case WatcherChangeTypes.Created:
                    TrackAdded?.Invoke(this, new TrackAddedEventArgs(watch.Path, GetTrack(e.FullPath)));
                    break;
                case WatcherChangeTypes.Deleted:
                    TrackRemoved?.Invoke(this, new TrackRemovedEventArgs(watch.Path, e.FullPath));
                    break;
            }
        }

        private void WatchOnRename(object sender, RenamedEventArgs e)
        {
            var watch = (FileSystemWatcher) sender;

            if (Directory.Exists(e.OldFullPath) || Directory.Exists(e.FullPath)) return;

            TrackRemoved?.Invoke(this, new TrackRemovedEventArgs(watch.Path, e.OldFullPath));
            TrackAdded?.Invoke(this, new TrackAddedEventArgs(watch.Path, GetTrack(e.FullPath)));
        }

        private class FilesystemLowTrack : LowTrack
        {
            public FilesystemLowTrack(string uri)
            {
                Uri = uri;
            }

            public override Stream GetStream() => File.OpenRead(Uri);
        }
    }
}