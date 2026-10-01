using ApiForge.Infrastructure.Parser;
using System.Text;

namespace ApiForge.Tests.Parser
{
    public class OpenApiParserTests
    {
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
    }
}
