using ApiForge.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ApiForge.ApplicationCore.DTOs.Responses;
using ApiForge.ApplicationCore.Enums;


namespace ApiForge.Tests.Controllers
{
    /// <summary>
    /// Unit tests for the <see cref="StatusController"/> class.
    /// </summary>
    public class StatusControllerTests
    {
        /// <summary>
        /// Tests that the Get method returns an OkObjectResult when called.
        /// </summary>
        [Fact]
        public void Get_ReturnsOk()
        {
            var controller = new StatusController();

            // Provide an HttpContext with a minimal IServiceProvider that returns an IWebHostEnvironment
            var services = new ServiceCollection();
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(new TestEnv { EnvironmentName = "Testing" });
            var provider = services.BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestServices = provider }
            };

            var result = controller.Get();
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// Tests that the Get method returns a StatusResponse with expected values.
        /// </summary>
        [Fact]
        public void Get_WhenCalled_ReturnsStatusResponseWithExpectedValues()
        {
            var controller = new StatusController();

            ServiceCollection services = new();
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(new TestEnv { EnvironmentName = "Testing" });
            var provider = services.BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestServices = provider }
            };

            var result = controller.Get();
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StatusResponse>(ok.Value);

            // Verify status and environment are set as expected
            Assert.Equal(ApiStatus.OK, response.Status);
            Assert.Equal("Testing", response.Environment, true);

            // Version should be present (either numeric or "unknown")
            Assert.False(string.IsNullOrWhiteSpace(response.Version));

            // Timestamps are recent
            var now = DateTime.UtcNow;
            Assert.InRange(response.UtcTimestamp, now - TimeSpan.FromSeconds(5), now + TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Tests that the Get method throws an InvalidOperationException when the IWebHostEnvironment is missing from the service provider.
        /// </summary>
        [Fact]
        public void Get_WhenEnvironmentMissing_ThrowsInvalidOperationException()
        {
            var controller = new StatusController();

            var provider = new ServiceCollection().BuildServiceProvider();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestServices = provider }
            };

            Assert.Throws<InvalidOperationException>(() => controller.Get());
        }
    }

    internal class TestEnv : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string WebRootPath { get; set; } = string.Empty;
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
