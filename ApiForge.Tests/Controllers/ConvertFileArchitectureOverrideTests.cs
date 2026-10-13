using ApiForge.Api.Controllers;
using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ApiForge.Tests.Controllers
{
    public class ConvertFileArchitectureOverrideTests
    {
        private sealed class CapturingGenerator : IApiForgeGenerator
        {
            public bool Called { get; private set; }
            public ArchitectureStyle? ReceivedOverride { get; private set; }

            public Task<GeneratedSolution> GenerateAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null) =>
                throw new NotSupportedException();

            public Task<GeneratedZipArchive> GenerateZipAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null)
            {
                Called = true;
                ReceivedOverride = architectureOverride;
                return Task.FromResult(new GeneratedZipArchive { Name = "Sol", Content = [1, 2, 3] });
            }
        }

        [Theory]
        [InlineData("hexagonal", ArchitectureStyle.Hexagonal)]
        [InlineData("ports-and-adapters", ArchitectureStyle.Hexagonal)]
        [InlineData("CLEAN", ArchitectureStyle.Clean)]
        public async Task Post_ExplicitArchitecture_IsForwarded(string value, ArchitectureStyle expected)
        {
            var generator = new CapturingGenerator();
            var controller = new ConvertFileToSolutionController(generator);

            var result = await controller.Post(TestHelpers.CreateFormFile("spec"), value);

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("Sol.zip", file.FileDownloadName);
            Assert.Equal(expected, generator.ReceivedOverride);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("auto")]
        [InlineData("AUTO")]
        public async Task Post_AutoOrEmptyArchitecture_SendsNoOverride(string? value)
        {
            var generator = new CapturingGenerator();
            var controller = new ConvertFileToSolutionController(generator);

            await controller.Post(TestHelpers.CreateFormFile("spec"), value);

            Assert.True(generator.Called);
            Assert.Null(generator.ReceivedOverride);
        }

        [Fact]
        public async Task Post_EmptyFile_ReturnsBadRequestWithoutCallingGenerator()
        {
            var generator = new CapturingGenerator();
            var controller = new ConvertFileToSolutionController(generator);

            var result = await controller.Post(TestHelpers.CreateFormFile(string.Empty), "auto");

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.False(generator.Called);
        }
    }
}