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

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/zip", fileResult.ContentType);

            using var ms = new MemoryStream(fileResult.FileContents);
            using var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read);
            Assert.Contains(true, archive.Entries.Select(e => !string.IsNullOrWhiteSpace(e.FullName)));
        }
    }
}
