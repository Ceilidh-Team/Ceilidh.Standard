using System;
using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Mount.FileSystem
{
    internal abstract class FileSystemMountRecord : IMountRecord
    {
        public string Name => Info.Name;
        public Uri Path => new Uri(Info.FullName);
        public IMountDriver ParentDriver => FileSystemMountDriver;

        protected FileSystemMountDriver FileSystemMountDriver { get; }
        protected abstract FileSystemInfo Info { get; }

        protected FileSystemMountRecord(FileSystemMountDriver parentDriver)
        {
            FileSystemMountDriver = parentDriver;
        }

        public void Refresh() => Info.Refresh();
        
        public abstract void Dispose();

        public override int GetHashCode() => Path.GetHashCode();
        
        protected bool Equals(FileSystemMountRecord other) => Path.Equals(other.Path);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((FileSystemMountRecord) obj);
        }
    }
}