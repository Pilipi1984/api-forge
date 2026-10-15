using ApiForge.Api.Controllers;
using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ApiForge.Tests.Controllers
{
    /// <summary>
    /// Tests for the ConvertFileToSolutionController to ensure that the architecture override is correctly forwarded to the generator.
    /// </summary>
    public class ConvertFileArchitectureOverrideTests
    {
        /// <summary>
        /// A capturing implementation of IApiForgeGenerator that records whether it was called and what architecture override it received.
        /// </summary>
        private sealed class CapturingGenerator : IApiForgeGenerator
        {
            /// <summary>
            /// Indicates whether the GenerateZipAsync method was called.
            /// </summary>
            public bool Called { get; private set; }
            /// <summary>
            /// The architecture override received by the GenerateZipAsync method, if any.
            /// </summary>
            public ArchitectureStyle? ReceivedOverride { get; private set; }

            /// <summary>
            /// This method is not supported in this test implementation and will throw a NotSupportedException if called.
            /// </summary>
            /// <param name="openApiSpec">The OpenAPI specification stream.</param>
            /// <param name="architectureOverride">The architecture override.</param>
            /// <returns>Generated solution.</returns>
            /// <exception cref="NotSupportedException"></exception>
            public Task<GeneratedSolution> GenerateAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null) =>
                throw new NotSupportedException();

            /// <summary>
            /// Captures the call to GenerateZipAsync, recording the architecture override and returning a dummy GeneratedZipArchive.
            /// </summary>
            /// <param name="openApiSpec">The OpenAPI specification stream.</param>
            /// <param name="architectureOverride">The architecture override.</param>
            /// <returns>Generated zip archive.</returns>
            public Task<GeneratedZipArchive> GenerateZipAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null)
            {
                Called = true;
                ReceivedOverride = architectureOverride;
                return Task.FromResult(new GeneratedZipArchive { Name = "Sol", Content = [1, 2, 3] });
            }
        }

        /// <summary>
        /// Tests that when a specific architecture style is provided in the request, it is correctly forwarded to the generator.
        /// </summary>
        /// <param name="value">The architecture style value.</param>
        /// <param name="expected">The expected architecture style.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Tests that when the architecture style is set to "auto" or is empty, no override is sent to the generator.
        /// </summary>
        /// <param name="value">The architecture style value.</param>
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

        /// <summary>
        /// Tests that when an empty file is posted, the controller returns a BadRequest result and does not call the generator.
        /// </summary>
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