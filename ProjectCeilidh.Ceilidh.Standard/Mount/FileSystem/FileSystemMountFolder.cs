using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ProjectCeilidh.Ceilidh.Standard.Mount.FileSystem
{
    internal class FileSystemMountFolder : FileSystemMountRecord, IMountFolder
    {
        protected override FileSystemInfo Info => _info;
        
        private FileSystemWatcher _watcher;
        private ObservableCollection<IMountRecord> _children;
        private ReadOnlyObservableCollection<IMountRecord> _readOnlyChildren;

        public ReadOnlyObservableCollection<IMountRecord> Children
        {
            get
            {
                if (_children != null) return _readOnlyChildren;
                
                _children = new ObservableCollection<IMountRecord>();

                foreach (var fileSystemInfo in _info.EnumerateFileSystemInfos())
                {
                    if (!FileSystemMountDriver.TryGetRecord(fileSystemInfo, out var record)) continue;
                    
                    _children.Add(record);
                }

                _watcher = new FileSystemWatcher
                {
                    Path = _info.FullName,
                    IncludeSubdirectories = false,
                };
                
                _watcher.Created += WatcherOnCreated;
                _watcher.Deleted += WatcherOnDeleted;
                _watcher.Renamed += WatcherOnRenamed;
                _watcher.Changed += WatcherOnChanged;

                _watcher.EnableRaisingEvents = true;
                
                return _readOnlyChildren = new ReadOnlyObservableCollection<IMountRecord>(_children);
            }
        }

        private readonly DirectoryInfo _info;
        
        public FileSystemMountFolder(FileSystemMountDriver parentDriver, DirectoryInfo info) : base(parentDriver)
        {
            _info = info;
        }
        
        public bool TryCreateFile(Uri name, out IMountFile file)
        {
            file = default;
            
            if (name.IsAbsoluteUri) return false;

            var newUri = new Uri(Path, name);
            File.Open(newUri.AbsolutePath, FileMode.Create, FileAccess.Read, FileShare.Read | FileShare.Write).Dispose();

            ParentDriver.TryGetRecord(newUri, out var record);
            if (!(record is IMountFile newFile)) return false;
            file = newFile;
            return true;
        }

        public bool TryCreateFolder(Uri name, out IMountFolder folder)
        {
            folder = default;
            
            if (name.IsAbsoluteUri) return false;

            var newUri = new Uri(Path, name);
            var info = Directory.CreateDirectory(newUri.AbsolutePath);

            FileSystemMountDriver.TryGetRecord(info, out var record);
            if (!(record is IMountFolder newFolder)) return false;
            folder = newFolder;
            return true;
        }

        public override void Dispose()
        {
            _watcher?.Dispose();
        }
        
        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (FileSystemMountDriver.TryGetRecord(new Uri(e.FullPath), out var record))
                _children.Add(record);
        }
        
        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            var item = _children.Single(x => x.Path.AbsolutePath == e.FullPath);
            _children.Remove(item);
        }
        
        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            var item = _children.Single(x => x.Path.AbsolutePath == e.OldFullPath);
            _children.Remove(item);
            
            if (ParentDriver.TryGetRecord(new Uri(e.FullPath), out var record))
                _children.Add(record);
        }
        
        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;
            
            var item = _children.Single(x => x.Path.AbsolutePath == e.FullPath);
            item.Refresh();
        }
    }
}