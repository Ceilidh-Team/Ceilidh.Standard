using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Ceilidh.Core.Plugin.Archive
{
    public class PluginArchive
    {
        public string ArchiveDirectory { get; }

        public PluginArchive(string archiveDir)
        {
            ArchiveDirectory = archiveDir;
            Directory.CreateDirectory(ArchiveDirectory);
        }

        internal IEnumerable<string> InstalledPlugins()
        {
            return Directory.EnumerateDirectories(ArchiveDirectory)
                .Select(x => Path.ChangeExtension(".dll", Path.Combine(x, Path.GetFileName(x))));

        }

        public string InstallPackage(string archiveName, Stream str)
        {
            var dir = Directory.CreateDirectory(Path.Combine(ArchiveDirectory, archiveName));

            using (var zip = new ZipArchive(str, ZipArchiveMode.Read, true))
                zip.ExtractToDirectory(dir.FullName);

            return Path.ChangeExtension(Path.Combine(dir.FullName, archiveName), ".dll");
        }
    }
}
