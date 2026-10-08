using ApiForge.Api.Controllers;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Parser;
using Microsoft.AspNetCore.Mvc;
using ApiForge.Tests.Helpers;

namespace ApiForge.Tests.Integration
{
    public class EndToEndTests
    {
        [Fact]
        public async Task FullPipeline_GeneratesZip()
        {
            var parser = new OpenApiParser();
            var generator = new CodeGenerator();
            var controller = new ConvertFileToSolutionController(new Generator.ApiForgeGenerator(parser, generator));

            var path = @"..\..\..\Integration\TestFiles\test.yml";

            var result = await controller.Post(path, "auto");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var filePath = doc.RootElement.GetProperty("filePath").GetString();
            Assert.False(string.IsNullOrWhiteSpace(filePath));
            Assert.True(System.IO.File.Exists(filePath));

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            using var ms = new MemoryStream(fileBytes);
            using var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read);
            Assert.Contains(true, archive.Entries.Select(e => !string.IsNullOrWhiteSpace(e.FullName)));
        }
    }
}
