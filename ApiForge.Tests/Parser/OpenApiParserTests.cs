using ApiForge.Infrastructure.Parser;

namespace ApiForge.Tests.Parser
{
    public class OpenApiParserTests
    {
        [Test]
        public async Task ParseAsync_ParsesSimpleYaml()
        {
            var yaml = "openapi: 3.0.0\ninfo:\n  title: Test\n  version: 1.0.0\npaths: {}";
            var parser = new OpenApiParser();

            using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(yaml));
            var result = await parser.ParseAsync(ms);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test", result.Title);
        }
    }
}
