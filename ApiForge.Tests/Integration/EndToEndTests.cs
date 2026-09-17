using ApiForge.Api.Controllers;
using ApiForge.Application.Interfaces;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using ApiForge.Tests.Helpers;

namespace ApiForge.Tests.Integration
{
    public class EndToEndTests
    {
        [Test]
        public async Task FullPipeline_GeneratesZip()
        {
            var parser = new OpenApiParser();
            var generator = new CodeGenerator();
            var controller = new ConvertFileToSolutionController(parser, generator);

            var yaml = "openapi: 3.0.0\ninfo:\n  title: ET\n  version: 1.0.0\npaths: {}";
            var file = TestHelpers.CreateFormFile(yaml);

            var result = await controller.Post(file, "auto", CancellationToken.None);

            Assert.IsInstanceOf<FileContentResult>(result);
            var fileResult = (FileContentResult)result;
            Assert.AreEqual("application/zip", fileResult.ContentType);

            using var ms = new MemoryStream(fileResult.FileContents);
            using var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read);
            Assert.IsTrue(archive.Entries.Any());
        }
    }
}
