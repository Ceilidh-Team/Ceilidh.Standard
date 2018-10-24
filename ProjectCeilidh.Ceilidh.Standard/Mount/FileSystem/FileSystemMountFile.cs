using System;
using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Mount.FileSystem
{
    internal class FileSystemMountFile : FileSystemMountRecord, IMountFile
    {
        public long? Length => _info.Length;
        public DateTime? DateCreated => _info.CreationTime;
        public DateTime? DateModified => _info.LastWriteTime;

        protected override FileSystemInfo Info => _info;

        private readonly FileInfo _info;
        
        public FileSystemMountFile(FileSystemMountDriver parentDriver, FileInfo info) : base(parentDriver)
        {
            _info = info;
        }
            
        public bool TryOpenRead(out Stream stream)
        {
            stream = default;
            
            if (!_info.Exists) return false;

            stream = File.Open(_info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            return true;
        }

        public bool TryOpenWrite(out Stream stream)
        {
            stream = default;

            if (_info.IsReadOnly) return false;

            stream = File.Open(_info.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            return true;
        }

        public override void Dispose() { }
    }
}