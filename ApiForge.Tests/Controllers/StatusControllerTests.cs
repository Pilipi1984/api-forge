using ApiForge.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiForge.ApplicationCore.DTOs.Responses;
using ApiForge.ApplicationCore.Enums;
using System;


namespace ApiForge.Tests.Controllers
{
    public class StatusControllerTests
    {
        [Fact]
        public void Get_ReturnsOk()
        {
            var controller = new StatusController();

            // Provide an HttpContext with a minimal IServiceProvider that returns an IWebHostEnvironment
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(new TestEnv { EnvironmentName = "Testing" });
            var provider = services.BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestServices = provider }
            };

            var result = controller.Get();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void Get_WhenCalled_ReturnsStatusResponseWithExpectedValues()
        {
            var controller = new StatusController();

            ServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
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
            Assert.Equal("Testing", response.Environment);

            // Version should be present (either numeric or "unknown")
            Assert.False(string.IsNullOrWhiteSpace(response.Version));

            // Timestamps are recent
            var now = DateTime.UtcNow;
            Assert.InRange(response.UtcTimestamp, now - TimeSpan.FromSeconds(5), now + TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Get_WhenEnvironmentMissing_ThrowsInvalidOperationException()
        {
            var controller = new StatusController();

            var provider = new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider();
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
