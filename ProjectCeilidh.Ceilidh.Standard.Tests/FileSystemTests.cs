using System.Collections.Specialized;
using System.IO;
using System.Threading;
using ProjectCeilidh.Ceilidh.Standard.Library;
using Xunit;

namespace ProjectCeilidh.Ceilidh.Standard.Tests
{
    public class FileSystemTests
    {
        private readonly FileSystemLibraryProvider _provider;

        public FileSystemTests()
        {
            _provider = new FileSystemLibraryProvider();
        }

        [Theory]
        [InlineData(true, "/absolute")]
        [InlineData(false, "relative")]
        public void CanAccept(bool canAccept, string uri)
        {
            Assert.Equal(canAccept, _provider.CanAccept(uri));
        }

        [Fact]
        public void GetSource()
        {
            var asmPath = typeof(FileSystemTests).Assembly.Location;

            Assert.True(_provider.TryGetSource(asmPath, out var s));
            using (var str = s.GetStream())
                Assert.True(str is FileStream f && f.Name == asmPath);
        }

        [Fact]
        public void MonitorDir()
        {
            var testDir = Path.GetFullPath("./test");
            Directory.CreateDirectory(testDir);

            var testFile = Path.Combine(testDir, "test.dat");

            if (File.Exists(testFile))
                File.Delete(testFile);

            Assert.True(_provider.TryGetLibraryCollection(testDir, out var sources));
            using (sources)
            {
                Assert.Equal(0, sources.Count);

                var handle = new EventWaitHandle(false, EventResetMode.AutoReset);

                sources.CollectionChanged += Set;

                File.Create(testFile).Dispose();

                Assert.True(handle.WaitOne(1000));
                Assert.Equal(1, sources.Count);

                sources.CollectionChanged -= Set;

                void Set(object sender, NotifyCollectionChangedEventArgs args) => handle.Set();
            }
        }
    }
}
