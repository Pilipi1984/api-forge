using ApiForge.Domain.Enums;
using ApiForge.Infrastructure.Helpers;
using Xunit;

namespace ApiForge.Tests.Helpers
{
    public class ArchitectureStyleParserTests
    {
        [Theory]
        [InlineData("hexagonal", ArchitectureStyle.Hexagonal)]
        [InlineData("ports-and-adapters", ArchitectureStyle.Hexagonal)]
        [InlineData("clean", ArchitectureStyle.Clean)]
        [InlineData("clean-architecture", ArchitectureStyle.Clean)]
        public void TryParse_KnownValues(string input, ArchitectureStyle expected)
        {
            var ok = ArchitectureStyleParser.TryParse(input, out var style);
            Assert.True(ok);
            Assert.Equal(expected, style);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("unknown")]
        public void TryParse_UnknownValues_ReturnsFalse(string? input)
        {
            var ok = ArchitectureStyleParser.TryParse(input, out var style);
            Assert.False(ok);
            Assert.Equal(ArchitectureStyle.Clean, style);
        }
    }
}
