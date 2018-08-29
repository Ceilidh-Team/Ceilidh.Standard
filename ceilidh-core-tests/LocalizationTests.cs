using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Ceilidh.Core.Plugin;
using Ceilidh.Core.Vendor.Contracts;
using Xunit;

namespace Ceilidh.Core.Tests
{
    public class LocalizationTests
    {
        private readonly ILocalizationController _localization;

        public LocalizationTests()
        {
            Thread.CurrentThread.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            var plug = new PluginLoader(new TestLocalizationPhraseProvider());
            plug.QueueLoad(typeof(PluginLoader).Assembly);
            var impl = plug.Execute(new string[0]);
            Assert.True(impl.TryGetSingleton(out _localization));
        }

        [Fact]
        public void PluralizationTest()
        {
            Assert.Equal("0 items", _localization.Translate("test.plural", 0));
            Assert.Equal("1 item", _localization.Translate("test.plural", 1));
            Assert.Equal("2 items", _localization.Translate("test.plural", 2));
        }

        [Fact]
        public void InterpolationTest()
        {
            Assert.Equal("Ceilidh, your name is Ceilidh!", _localization.Translate("test.interpolate", "Ceilidh"));
        }

        [Fact]
        public void MissingKeyTest()
        {
            Assert.Equal("test.missing", _localization.Translate("test.missing"));
        }

        [Fact]
        public void MissingValueTest()
        {
            Assert.Equal("{0} {1}", _localization.Translate("test.multi"));
        }

        private class TestLocalizationPhraseProvider : ILocalizationPhraseProvider
        {
            public IReadOnlyDictionary<string, string[]> GetPhrases(CultureInfo culture)
            {
                switch (culture)
                {
                    case var c when c.TwoLetterISOLanguageName == "en" || c.Equals(CultureInfo.InvariantCulture):
                        return new Dictionary<string, string[]>
                        {
                            ["test.plural"] = new[] { "{0} item", "{0} items" },
                            ["test.interpolate"] = new[] { "{0}, your name is {0}!" },
                            ["test.multi"] = new[] { "{0} {1}" }
                        };
                    default: return null;
                }
            }
        }
    }
}
