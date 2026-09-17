using ApiForge.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace ApiForge.Tests.Controllers
{
    public class StatusControllerTests
    {
        [Test]
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
            Assert.IsInstanceOf<OkObjectResult>(result);
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
