using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ProjectCeilidh.Ceilidh.Standard.Localization;
using Xunit;

namespace ProjectCeilidh.Ceilidh.Standard.Tests
{
    public class PolyglotTests
    {
        private readonly ILocalizationController _localization;

        public PolyglotTests()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            _localization = new PolyglotLocalizationController(new ILocalizationPhraseProvider[0]);
            _localization.Extend(new Dictionary<string, string[]>
            {
                ["test.plural"] = new[] { "{0} item", "{0} items" },
                ["test.interpolate"] = new[] { "{0}, your name is {0}!" },
                ["test.multi"] = new[] { "{0} {1}" }
            });
        }

        [Fact]
        public void Pluralization()
        {
            Assert.Equal("0 items", _localization.Translate("test.plural", 0));
            Assert.Equal("1 item", _localization.Translate("test.plural", 1));
            Assert.Equal("2 items", _localization.Translate("test.plural", 2));
        }

        [Fact]
        public void Interpolation()
        {
            Assert.Equal("Ceilidh, your name is Ceilidh!", _localization.Translate("test.interpolate", "Ceilidh"));
        }

        [Fact]
        public void MissingKey()
        {
            Assert.Equal("test.missing", _localization.Translate("test.missing"));
        }

        [Fact]
        public void MissingValue()
        {
            Assert.Equal("{0} {1}", _localization.Translate("test.multi"));
        }
    }
}
