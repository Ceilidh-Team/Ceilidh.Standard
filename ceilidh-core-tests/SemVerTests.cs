using System;
using Ceilidh.Core.Util;
using Xunit;

namespace Ceilidh.Core.Tests
{
    public class SemVerTests
    {
        [Theory]
        [InlineData("1.0.0", "1.0.0", true)]
        [InlineData("1.0.0", "0.0.0", false)]
        [InlineData("1.0.0", "2.0.0", false)]
        [InlineData("1.0.0", "1.1.0", true)]
        [InlineData("1.0.0", "1.1.1", true)]
        [InlineData("1.1.0", "1.0.0", false)]
        [InlineData("1.1.1", "1.1.0", false)]
        [InlineData("1.1.1", "1.2.0", true)]
        [InlineData("1.2.0", "1.2.1", true)]
        public void Validate(string pattern, string value, bool expected)
        {
            Assert.Equal(expected, new Version(pattern).AreEquivalent(new Version(value)));
        }
    }
}
