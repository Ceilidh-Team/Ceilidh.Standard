using Ceilidh.Core.Plugin;
using Xunit;

namespace Ceilidh.Core.Tests
{
    public class VendorPluginLoaderTests
    {
        [Fact]
        public void VendorLoad()
        {
            var plug = new PluginLoader();
            plug.QueueLoad(typeof(PluginLoader).Assembly);
            plug.Execute(new string[0]);
        }
    }
}
