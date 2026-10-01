using ApiForge.Infrastructure.Helpers;
using Xunit;

namespace ApiForge.Tests.Helpers
{
    public class NameHelperTests
    {
        [Theory]
        [InlineData(null, "Unnamed")]
        [InlineData("", "Unnamed")]
        [InlineData("simple", "Simple")]
        [InlineData("my api", "MyApi")]
        [InlineData("123abc", "_123abc")]
        [InlineData("hello-world", "Hello-world")]
        public void ToPascalCase_Works(string? input, string expected)
        {
            var actual = NameHelper.ToPascalCase(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("MyValue", "myValue")]
        [InlineData("another test", "anotherTest")]
        public void ToCamelCase_Works(string input, string expected)
        {
            var actual = NameHelper.ToCamelCase(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("class", "@class")]
        [InlineData("valueName", "valueName")]
        [InlineData(null, "unnamed")]
        public void ToValidIdentifier_Works(string? input, string expected)
        {
            var actual = NameHelper.ToValidIdentifier(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, "GeneratedApi")]
        [InlineData("", "GeneratedApi")]
        [InlineData("Acme.App", "Acme.App")]
        [InlineData("123root.name", "_123root.name")]
        public void ToSolutionName_Works(string? input, string expected)
        {
            var actual = NameHelper.ToSolutionName(input);
            Assert.Equal(expected, actual);
        }
    }
}
