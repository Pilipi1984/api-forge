using ApiForge.Api.Controllers;
using ApiForge.Application.Interfaces;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;

namespace ApiForge.Tests.Controllers
{
    public class ConvertFileToSolutionControllerTests
    {
        [Fact]
        public async Task Post_Returns_BadRequest_When_NoFile()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator());

            var result = await controller.Post(null, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task Post_Returns_BadRequest_When_ParserThrows()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator(shouldThrow: true));

            var file = CreateFormFile("content");
            var result = await controller.Post(file, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Failed to generate solution", bad.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Post_Returns_BadRequest_When_InvalidArchitecture()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator());

            var file = CreateFormFile("content");
            var result = await controller.Post(file, "invalid-arch");

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unrecognized architecture value", bad.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Post_Returns_ZipFile_OnSuccess()
        {
            var generator = new DummyGenerator();
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator(generator.Solution));

            var file = CreateFormFile("content");
            var result = await controller.Post(file, null);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/zip", fileResult.ContentType);
            Assert.Equal($"{generator.Solution.Name}.zip", fileResult.FileDownloadName);

            // Verify zip content contains the generated file
            using var ms = new MemoryStream(fileResult.FileContents);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            var entry = archive.GetEntry(generator.Solution.Files[0].RelativePath);
            Assert.NotNull(entry);
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            var text = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            Assert.Equal(generator.Solution.Files[0].Content, text);
        }

        private class FakeApiForgeGenerator : IApiForgeGenerator
        {
            private readonly GeneratedSolution? _solution;
            private readonly bool _shouldThrow;

            public FakeApiForgeGenerator(GeneratedSolution? solution = null, bool shouldThrow = false)
            {
                _solution = solution;
                _shouldThrow = shouldThrow;
            }

            public Task<GeneratedSolution> GenerateAsync(Stream openApiSpec, ApiForge.Domain.Enums.ArchitectureStyle? architectureOverride = null)
            {
                if (_shouldThrow) throw new InvalidOperationException("parse failed");
                return Task.FromResult(_solution ?? new GeneratedSolution { Name = "Stub", RootNamespace = "R", Files = new List<GeneratedFile>() });
            }

            public async Task<GeneratedZipArchive> GenerateZipAsync(Stream openApiSpec, ApiForge.Domain.Enums.ArchitectureStyle? architectureOverride = null)
            {
                if (_shouldThrow) throw new InvalidOperationException("parse failed");

                var sol = _solution ?? new GeneratedSolution { Name = "Stub", RootNamespace = "R", Files = new List<GeneratedFile>() };
                using var ms = new MemoryStream();
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var file in sol.Files)
                    {
                        var entry = archive.CreateEntry(file.RelativePath, CompressionLevel.Optimal);
                        await using var entryStream = entry.Open();
                        await using var writer = new StreamWriter(entryStream);
                        await writer.WriteAsync(file.Content);
                    }
                }

                return new GeneratedZipArchive { Name = sol.Name, Content = ms.ToArray() };
            }
        }

        private static IFormFile CreateFormFile(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var ms = new MemoryStream(bytes);
            return new FormFile(ms, 0, bytes.Length, "file", "test.yaml");
        }

        private class DummyParser : IOpenApiParser
        {
            public Task<ApiDefinition> ParseAsync(Stream stream)
            {
                var def = new ApiDefinition { Title = "Test", Version = "1.0" };
                return Task.FromResult(def);
            }
        }

        private class ThrowingParser : IOpenApiParser
        {
            public Task<ApiDefinition> ParseAsync(Stream stream)
            {
                throw new InvalidOperationException("parse failed");
            }
        }

        private class DummyGenerator : ICodeGenerator
        {
            public GeneratedSolution Solution { get; }

            public DummyGenerator()
            {
                Solution = new GeneratedSolution
                {
                    Name = "MySolution",
                    RootNamespace = "MyRoot",
                    Files = new List<GeneratedFile>
                    {
                        new() { RelativePath = "Project/Program.cs", Content = "console" }
                    }
                };
            }

            public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
            {
                return Task.FromResult(Solution);
            }
        }
    }
}
