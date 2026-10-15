using ApiForge.Infrastructure.Parser;
using System.Text;

namespace ApiForge.Tests.Parser
{
    /// <summary>
    /// Unit tests for the <see cref="OpenApiParser"/> class.
    /// </summary>
    public class OpenApiParserTests
    {
        /// <summary>
        /// Tests that the <see cref="OpenApiParser.ParseAsync(Stream)"/> method correctly parses a simple OpenAPI YAML document.
        /// </summary>
        [Fact]
        public async Task ParseAsync_ParsesSimpleYaml()
        {
            var yaml = "openapi: 3.0.0\ninfo:\n  title: Test\n  version: 1.0.0\npaths: {}";
            var parser = new OpenApiParser();

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
            var result = await parser.ParseAsync(ms);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Title);
        }

        /// <summary>
        /// Tests that the <see cref="OpenApiParser.ParseAsync(Stream)"/> method throws an exception when given a null or empty YAML document.
        /// </summary>
        [Fact]
        public async Task ParseAsync_NullYaml()
        {
            var parser = new OpenApiParser();

            Assert.Throws<AggregateException>(() => parser.ParseAsync(new MemoryStream()).Result);
        }
    }
}
