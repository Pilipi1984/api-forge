using ApiForge.Domain.Enums;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Tests.Helpers
{
    /// <summary>
    /// Tests for the ArchitectureStyleParser class
    /// </summary>
    public class ArchitectureStyleParserTests
    {
        /// <summary>
        /// Tests the TryParse method with known values and checks if the output matches the expected ArchitectureStyle enum value.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        /// <param name="expected">The expected ArchitectureStyle enum value</param>
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

        /// <summary>
        /// Tests the TryParse method with unknown values and checks if the 
        /// output is false and the style is set to the default value (ArchitectureStyle.Clean).
        /// </summary>
        /// <param name="input">The input string to parse</param>
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
