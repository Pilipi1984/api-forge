using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Tests.Helpers
{
    /// <summary>
    /// Unit tests for the <see cref="NameHelper"/> class.
    /// </summary>
    public class NameHelperTests
    {
        /// <summary>
        /// Tests the <see cref="NameHelper.ToPascalCase(string?)"/> method with various input strings 
        /// and verifies that the output matches the expected PascalCase format.
        /// </summary>
        /// <param name="input">The input string to convert</param>
        /// <param name="expected">The expected PascalCase string</param>
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

        /// <summary>
        /// Tests the <see cref="NameHelper.ToCamelCase(string)"/> method with various input strings
        /// </summary>
        /// <param name="input">The input string to convert</param>
        /// <param name="expected">The expected camelCase string</param>
        [Theory]
        [InlineData("MyValue", "myValue")]
        [InlineData("another test", "anotherTest")]
        public void ToCamelCase_Works(string input, string expected)
        {
            var actual = NameHelper.ToCamelCase(input);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests the ToValidIdentifier method with various input strings
        /// </summary>
        /// <param name="input">The input string to convert</param>
        /// <param name="expected">The expected valid identifier string</param>
        [Theory]
        [InlineData("class", "@class")]
        [InlineData("valueName", "valueName")]
        [InlineData(null, "unnamed")]
        public void ToValidIdentifier_Works(string? input, string expected)
        {
            var actual = NameHelper.ToValidIdentifier(input);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests the ToSolutionName method with various input strings
        /// </summary>
        /// <param name="input">The input string to convert</param>
        /// <param name="expected">The expected solution name string</param>
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
