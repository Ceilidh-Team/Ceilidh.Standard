using System;
using System.Collections.Generic;
using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Mount.FileSystem
{
    internal class FileSystemMountFolder : FileSystemMountRecord, IMountFolder
    {
        protected override FileSystemInfo Info => _info;
        
        public IReadOnlyCollection<IMountRecord> Children
        {
            get
            {
                var list = new List<IMountRecord>();

                foreach (var item in _info.EnumerateFileSystemInfos())
                {
                    if (!FileSystemMountDriver.TryGetRecord(item, out var record)) continue;
                    
                    list.Add(record);
                }

                return list;
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

        public override void Dispose() { }
    }
}