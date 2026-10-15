using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Generator;
using System.IO.Compression;
using System.Text;

namespace ApiForge.Tests.Generators
{
    /// <summary>
    /// Tests for the ApiForgeGenerator class
    /// </summary>
    public class ApiForgeGeneratorTests
    {
        /// <summary>
        /// Tests that the GenerateAsync method applies the architecture override correctly when generating a solution.
        /// </summary>
        [Fact]
        public async Task GenerateAsync_Applies_ArchitectureOverride()
        {
            var parser = new ParserReturningDefinition(ArchitectureStyle.Hexagonal);
            var generator = new CapturingGenerator();
            var apiForge = new ApiForgeGenerator(parser, generator);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("spec"));
            var solution = await apiForge.GenerateAsync(ms, ArchitectureStyle.Clean);

            Assert.NotNull(generator.ReceivedDefinition);
            var received = generator.ReceivedDefinition!;
            Assert.Equal(ArchitectureStyle.Clean, received.Architecture);
        }

        /// <summary>
        /// Tests that the GenerateZipAsync method correctly packages the generated files into a zip archive.
        /// </summary>
        [Fact]
        public async Task GenerateZipAsync_PackagesFilesCorrectly()
        {
            var parser = new ParserReturningDefinition();
            var generator = new ReturningGeneratorWithFiles();
            var apiForge = new ApiForgeGenerator(parser, generator);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("spec"));
            var zip = await apiForge.GenerateZipAsync(ms);

            Assert.Equal("MySolution", zip.Name);
            Assert.NotNull(zip.Content);

            using var zipMs = new MemoryStream(zip.Content);
            using var archive = new ZipArchive(zipMs, ZipArchiveMode.Read);
            var entry = archive.GetEntry("Project/Program.cs");
            Assert.NotNull(entry);
            var entryNonNull = entry!;
            using var reader = new StreamReader(entryNonNull.Open(), Encoding.UTF8);
            var content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            Assert.Equal("console", content);
        }

        /// <summary>
        /// Tests that the GenerateAsync method throws an ArgumentNullException when a null stream is passed.
        /// </summary>
        [Fact]
        public async Task GenerateAsync_Throws_OnNullStream()
        {
            var parser = new ParserReturningDefinition();
            var generator = new ReturningGeneratorWithFiles();
            var apiForge = new ApiForgeGenerator(parser, generator);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await apiForge.GenerateAsync(null!));
        }

        /// <summary>
        /// Tests that the GenerateZipAsync method throws an ArgumentNullException when a null stream is passed.
        /// </summary>
        [Fact]
        public async Task GenerateZipAsync_Throws_OnNullStream()
        {
            var parser = new ParserReturningDefinition();
            var generator = new ReturningGeneratorWithFiles();
            var apiForge = new ApiForgeGenerator(parser, generator);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await apiForge.GenerateZipAsync(null!));
        }

        /// <summary>
        /// Tests that the GenerateZipAsync method throws an ArgumentException when an empty stream is passed.
        /// </summary>
        /// <param name="arch">the architecture style</param>
        private class ParserReturningDefinition(ArchitectureStyle arch = ArchitectureStyle.Clean) : IOpenApiParser
        {
            private readonly ArchitectureStyle _arch = arch;

            /// <summary>
            /// Parses the provided stream and returns a dummy ApiDefinition with the specified architecture style.
            /// </summary>
            /// <param name="stream">The stream to parse.</param>
            /// <returns>The parsed ApiDefinition.</returns>
            public Task<ApiDefinition> ParseAsync(Stream stream)
            {
                var def = new ApiDefinition { Title = "T", Version = "1.0", Architecture = _arch };
                return Task.FromResult(def);
            }
        }

        /// <summary>
        /// A code generator that captures the received ApiDefinition for testing purposes.
        /// </summary>
        private class CapturingGenerator : ICodeGenerator
        {
            public ApiDefinition? ReceivedDefinition { get; private set; }

            /// <summary>
            /// Generates a dummy solution and captures the received ApiDefinition.
            /// </summary>
            /// <param name="definition">The ApiDefinition to capture.</param>
            /// <returns>The generated solution.</returns>
            public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
            {
                ReceivedDefinition = definition;
                var sol = new GeneratedSolution { Name = "S", RootNamespace = "R", Files = new List<GeneratedFile>() };
                return Task.FromResult(sol);
            }
        }

        /// <summary>
        /// A code generator that returns a predefined solution with files for testing purposes.
        /// </summary>
        private class ReturningGeneratorWithFiles : ICodeGenerator
        {
            /// <summary>
            /// Generates a predefined solution with files, ignoring the provided ApiDefinition.
            /// </summary>
            /// <param name="definition">The ApiDefinition to ignore.</param>
            /// <returns>The generated solution.</returns>
            public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
            {
                var sol = new GeneratedSolution
                {
                    Name = "MySolution",
                    RootNamespace = "MyRoot",
                    Files =
                    [
                        new() { RelativePath = "Project/Program.cs", Content = "console" }
                    ]
                };

                return Task.FromResult(sol);
            }
        }
    }
}
