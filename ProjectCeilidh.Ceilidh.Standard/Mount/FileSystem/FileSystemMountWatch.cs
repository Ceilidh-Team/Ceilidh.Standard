using System;
using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Mount.FileSystem
{
    internal class FileSystemMountWatch : IMountWatch
    {
        public Uri Path { get; }

        private readonly FileSystemWatcher _watcher;
        
        public FileSystemMountWatch(Uri path)
        {
            Path = path;
            
            _watcher = new FileSystemWatcher
            {
                Path = path.AbsolutePath,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.Security
                               | NotifyFilters.Size
                               | NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
            };
            
            _watcher.Changed += WatcherOnChanged;
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;
            
            
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}