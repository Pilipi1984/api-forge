using ApiForge.Api.Controllers;
using ApiForge.Application.Interfaces;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;

namespace ApiForge.Tests.Controllers
{
    public class ConvertFileToSolutionControllerTests
    {
        private const string Path = @"..\..\..\Controllers\TestFiles\test.yml";

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

            var result = await controller.Post(Path, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Failed to generate solution", bad.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Post_Returns_BadRequest_When_InvalidArchitecture()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator());

            var result = await controller.Post(Path, "invalid-arch");

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unrecognized architecture value", bad.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Post_Returns_ZipFile_OnSuccess()
        {
            var generator = new DummyGenerator();
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator(generator.Solution));

            var result = await controller.Post(Path, null);

            var okObject = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okObject.Value);
            var filePathProperty = okObject.Value.GetType().GetProperty("filePath");
            Assert.NotNull(filePathProperty);
            var filePath = filePathProperty.GetValue(okObject.Value) as string;
            Assert.False(string.IsNullOrEmpty(filePath));
        }

        [Fact]
        public async Task Post_Returns_BadRequest_When_FileNotFound()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator());

            var result = await controller.Post("this-file-does-not-exist.openapi", null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("File not found", bad.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Post_Returns_BadRequest_When_CreateFormFile_ThrowsIOException()
        {
            var temp = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllText(temp, "data");
                // Open exclusive lock to cause File.OpenRead in CreateFormFileFromPathAsync to fail with IOException
                using var lockStream = new System.IO.FileStream(temp, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None);

                var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator());

                var result = await controller.Post(temp, null);

                var bad = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Contains("Invalid file", bad.Value?.ToString() ?? string.Empty);
            }
            finally
            {
                try { System.IO.File.Delete(temp); } catch { }
            }
        }

        [Fact]
        public async Task Post_Returns_BadRequest_When_File_Is_Empty()
        {
            var temp = System.IO.Path.GetTempFileName();
            try
            {
                // ensure zero length
                System.IO.File.WriteAllBytes(temp, System.Array.Empty<byte>());

                var mockGen = new Moq.Mock<IApiForgeGenerator>();
                var controller = new ConvertFileToSolutionController(mockGen.Object);

                var result = await controller.Post(temp, null);

                var bad = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Contains("Upload an OpenAPI file", bad.Value?.ToString() ?? string.Empty);

                mockGen.Verify(g => g.GenerateZipAsync(Moq.It.IsAny<System.IO.Stream>(), Moq.It.IsAny<ApiForge.Domain.Enums.ArchitectureStyle?>()), Moq.Times.Never);
            }
            finally
            {
                try { System.IO.File.Delete(temp); } catch { }
            }
        }

        [Fact]
        public async Task Post_Passes_ArchitectureOverride_To_Generator_When_Explicit()
        {
            var temp = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllText(temp, "content");

                var returned = new ApiForge.Domain.GeneratedApiSolution.GeneratedZipArchive { Name = "X", Content = new byte[] { 1, 2, 3 } };
                ApiForge.Domain.Enums.ArchitectureStyle? captured = null;

                var mockGen = new Moq.Mock<IApiForgeGenerator>();
                mockGen.Setup(g => g.GenerateZipAsync(Moq.It.IsAny<System.IO.Stream>(), Moq.It.IsAny<ApiForge.Domain.Enums.ArchitectureStyle?>()))
                       .Callback<System.IO.Stream, ApiForge.Domain.Enums.ArchitectureStyle?>((s, a) => captured = a)
                       .Returns(System.Threading.Tasks.Task.FromResult(returned));

                var controller = new ConvertFileToSolutionController(mockGen.Object);

                var result = await controller.Post(temp, "clean");

                var ok = Assert.IsType<OkObjectResult>(result);
                Assert.NotNull(ok.Value);
                Assert.Equal(ApiForge.Domain.Enums.ArchitectureStyle.Clean, captured);

                mockGen.Verify(g => g.GenerateZipAsync(Moq.It.IsAny<System.IO.Stream>(), Moq.It.IsAny<ApiForge.Domain.Enums.ArchitectureStyle?>()), Moq.Times.Once);
            }
            finally
            {
                try { System.IO.File.Delete(temp); } catch { }
            }
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
                    Files =
                    [
                        new() { RelativePath = "Project/Program.cs", Content = "console" }
                    ]
                };
            }

            public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
            {
                return Task.FromResult(Solution);
            }
        }
    }
}
