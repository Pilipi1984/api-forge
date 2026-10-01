using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Generator;
using Xunit;
using System.IO.Compression;
using System.Text;

namespace ApiForge.Tests.Generators
{
    public class ApiForgeGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_Applies_ArchitectureOverride()
        {
            var parser = new ParserReturningDefinition(ArchitectureStyle.Hexagonal);
            var generator = new CapturingGenerator();
            var apiForge = new ApiForgeGenerator(parser, generator);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("spec"));
            var solution = await apiForge.GenerateAsync(ms, ArchitectureStyle.Clean);

            Assert.NotNull(generator.ReceivedDefinition);
            Assert.Equal(ArchitectureStyle.Clean, generator.ReceivedDefinition.Architecture);
        }

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
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("console", content);
        }

        [Fact]
        public async Task GenerateAsync_Throws_OnNullStream()
        {
            var parser = new ParserReturningDefinition();
            var generator = new ReturningGeneratorWithFiles();
            var apiForge = new ApiForgeGenerator(parser, generator);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await apiForge.GenerateAsync(null!));
        }

        private class ParserReturningDefinition : IOpenApiParser
        {
            private readonly ArchitectureStyle _arch;

            public ParserReturningDefinition(ArchitectureStyle arch = ArchitectureStyle.Clean)
            {
                _arch = arch;
            }

            public Task<ApiDefinition> ParseAsync(Stream stream)
            {
                var def = new ApiDefinition { Title = "T", Version = "1.0", Architecture = _arch };
                return Task.FromResult(def);
            }
        }

        private class CapturingGenerator : ICodeGenerator
        {
            public ApiDefinition? ReceivedDefinition { get; private set; }

            public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
            {
                ReceivedDefinition = definition;
                var sol = new GeneratedSolution { Name = "S", RootNamespace = "R", Files = new List<GeneratedFile>() };
                return Task.FromResult(sol);
            }
        }

        private class ReturningGeneratorWithFiles : ICodeGenerator
        {
            public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
            {
                var sol = new GeneratedSolution
                {
                    Name = "MySolution",
                    RootNamespace = "MyRoot",
                    Files = new List<GeneratedFile>
                    {
                        new GeneratedFile { RelativePath = "Project/Program.cs", Content = "console" }
                    }
                };

                return Task.FromResult(sol);
            }
        }
    }
}
