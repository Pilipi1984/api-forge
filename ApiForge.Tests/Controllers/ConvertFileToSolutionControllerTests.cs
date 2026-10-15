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
    /// <summary>
    /// Tests for the ConvertFileToSolutionController
    /// </summary>
    public class ConvertFileToSolutionControllerTests
    {
        /// <summary>
        /// Tests that the Post method returns a BadRequest result when no file is provided.
        /// </summary>
        [Fact]
        public async Task Post_Returns_BadRequest_When_NoFile()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator());

            var result = await controller.Post(null, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);
        }

        /// <summary>
        /// Tests that the Post method returns a BadRequest result when the parser throws an exception.
        /// </summary>
        [Fact]
        public async Task Post_Returns_BadRequest_When_ParserThrows()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator(shouldThrow: true));

            var file = CreateFormFile("content");
            var result = await controller.Post(file, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Failed to generate solution", bad.Value?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Tests that the Post method returns a BadRequest result when an invalid architecture value is provided.
        /// </summary>
        [Fact]
        public async Task Post_Returns_BadRequest_When_InvalidArchitecture()
        {
            var controller = new ConvertFileToSolutionController(new FakeApiForgeGenerator());

            var file = CreateFormFile("content");
            var result = await controller.Post(file, "invalid-arch");

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unrecognized architecture value", bad.Value?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Tests that the Post method returns a FileContentResult with a zip file when the generation is successful.
        /// </summary>
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
            var entryNonNull = entry!;
            using var reader = new StreamReader(entryNonNull.Open(), Encoding.UTF8);
            var text = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            Assert.Equal(generator.Solution.Files[0].Content, text);
        }

        /// <summary>
        /// A fake implementation of IApiForgeGenerator for testing purposes. It can be configured to return a predefined solution or throw an exception to simulate failure scenarios.
        /// </summary>
        /// <param name="solution">The predefined solution to return.</param>
        /// <param name="shouldThrow">Indicates whether to throw an exception.</param>
        private class FakeApiForgeGenerator(GeneratedSolution? solution = null, bool shouldThrow = false) : IApiForgeGenerator
        {
            private readonly GeneratedSolution? _solution = solution;
            private readonly bool _shouldThrow = shouldThrow;

            /// <summary>
            /// Generates a solution from the provided OpenAPI specification stream. If configured to throw, it will throw an InvalidOperationException to simulate a failure scenario.
            /// </summary>
            /// <param name="openApiSpec">The OpenAPI specification stream.</param>
            /// <param name="architectureOverride">The architecture override.</param>
            /// <returns>The generated solution.</returns>
            /// <exception cref="InvalidOperationException">Thrown when the generator is configured to throw an exception.</exception>
            public Task<GeneratedSolution> GenerateAsync(Stream openApiSpec, ApiForge.Domain.Enums.ArchitectureStyle? architectureOverride = null)
            {
                if (_shouldThrow) throw new InvalidOperationException("parse failed");
                return Task.FromResult(_solution ?? new GeneratedSolution { Name = "Stub", RootNamespace = "R", Files = new List<GeneratedFile>() });
            }

            /// <summary>
            /// Generates a zip archive from the provided OpenAPI specification stream. If configured to throw, it will throw an InvalidOperationException to simulate a failure scenario.
            /// </summary>
            /// <param name="openApiSpec">The OpenAPI specification stream.</param>
            /// <param name="architectureOverride">The architecture override.</param>
            /// <returns>The generated zip archive.</returns>
            /// <exception cref="InvalidOperationException">Thrown when the generator is configured to throw an exception.</exception>
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

        /// <summary>
        /// Creates a FormFile from the given string content. This is used to simulate file uploads in tests.
        /// </summary>
        /// <param name="content">The content of the file.</param>
        /// <returns>The created FormFile.</returns>
        private static FormFile CreateFormFile(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var ms = new MemoryStream(bytes);
            return new FormFile(ms, 0, bytes.Length, "file", "test.yaml");
        }

        /// <summary>
        /// A dummy implementation of IOpenApiParser that returns a predefined ApiDefinition. 
        /// This is used for testing purposes to simulate successful parsing of an OpenAPI specification.
        /// </summary>
        private class DummyParser : IOpenApiParser
        {
            /// <summary>
            /// Parses the provided stream and returns a predefined ApiDefinition. 
            /// This simulates successful parsing of an OpenAPI specification for testing purposes.
            /// </summary>
            /// <param name="stream">The stream to parse.</param>
            /// <returns>The parsed ApiDefinition.</returns>
            public Task<ApiDefinition> ParseAsync(Stream stream)
            {
                var def = new ApiDefinition { Title = "Test", Version = "1.0" };
                return Task.FromResult(def);
            }
        }

        /// <summary>
        /// A dummy implementation of IOpenApiParser that throws an InvalidOperationException when ParseAsync is called. 
        /// This is used for testing error handling in the controller.
        /// </summary>
        private class ThrowingParser : IOpenApiParser
        {
            /// <summary>
            /// Parses the provided stream and throws an InvalidOperationException to simulate a parsing failure. 
            /// This is used for testing error handling in the controller.
            /// </summary>
            /// <param name="stream">The stream to parse.</param>
            /// <returns>The parsed ApiDefinition.</returns>
            /// <exception cref="InvalidOperationException"></exception>
            public Task<ApiDefinition> ParseAsync(Stream stream)
            {
                throw new InvalidOperationException("parse failed");
            }
        }

        /// <summary>
        /// A dummy implementation of ICodeGenerator that returns a predefined GeneratedSolution. 
        /// This is used for testing purposes to simulate successful code generation from an OpenAPI specification.
        /// </summary>
        private class DummyGenerator : ICodeGenerator
        {
            public GeneratedSolution Solution { get; }

            /// <summary>
            /// Initializes a new instance of the DummyGenerator class with a predefined GeneratedSolution.
            /// </summary>
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

            /// <summary>
            /// Generates a solution from the provided ApiDefinition. This implementation returns a predefined GeneratedSolution for testing purposes.
            /// </summary>
            /// <param name="definition"></param>
            /// <returns></returns>
            public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
            {
                return Task.FromResult(Solution);
            }
        }
    }
}
