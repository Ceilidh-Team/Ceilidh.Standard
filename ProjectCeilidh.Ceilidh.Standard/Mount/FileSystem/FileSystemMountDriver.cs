using System;
using System.IO;
using ProjectCeilidh.Ceilidh.Standard.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Mount.FileSystem
{
    [CobbleExport]
    public class FileSystemMountDriver : IMountDriver
    {
        public bool CanAccept(Uri uri) => uri.Scheme == Uri.UriSchemeFile;

        public bool TryGetRecord(Uri uri, out IMountRecord record)
        {
            return File.GetAttributes(uri.AbsolutePath).HasFlag(FileAttributes.Directory)
                ? TryGetRecord(new DirectoryInfo(uri.AbsolutePath), out record)
                : TryGetRecord(new FileInfo(uri.AbsolutePath), out record);
        }

        internal bool TryGetRecord(FileSystemInfo info, out IMountRecord record)
        {
            record = default;
            
            switch (info)
            {
                case FileInfo fileInfo:
                    record = new FileSystemMountFile(this, fileInfo);
                    return true;
                case DirectoryInfo directoryInfo:
                    record = new FileSystemMountFolder(this, directoryInfo);
                    return true;
                default:
                    return false;
            }
        }

        public bool TryWatchFolder(Uri uri, out IMountWatch watch)
        {
            watch = default;
            
            if (!Directory.Exists(uri.AbsolutePath)) return false;
            
            watch = new FileSystemMountWatch(uri);
            return true;
        }
    }
}