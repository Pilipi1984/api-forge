using Microsoft.OpenApi;
using Xunit;
using ApiForge.Domain.Models.ApiParameters;

namespace ApiForge.Tests.Models.ApiParameters
{
    public class ApiHeaderParameterTests
    {
        [Fact]
        public void ApiHeaderParameter_Constructor_SetsLocationToHeader()
        {
            // Arrange & Act
            // Use Activator to bypass 'required' member compile-time enforcement so we can test
            // the constructor behavior that sets Location to Header.
            var parameter = (ApiHeaderParameter)Activator.CreateInstance(typeof(ApiHeaderParameter))!;

            // Assert
            Assert.Equal(ParameterLocation.Header, parameter.Location);
        }

        [Fact]
        public void ApiHeaderParameter_Constructor_ObjectInitializerOverridesConstructorLocation()
        {
            // Arrange & Act
            var parameter = new ApiHeaderParameter { Name = "Y", Type = "int", Location = ParameterLocation.Query };

            // Assert
            Assert.Equal(ParameterLocation.Query, parameter.Location);
        }
    }
}
