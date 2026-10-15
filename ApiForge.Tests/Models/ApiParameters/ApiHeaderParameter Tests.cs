using Microsoft.OpenApi;
using ApiForge.Domain.Models.ApiParameters;

namespace ApiForge.Tests.Models.ApiParameters
{
    /// <summary>
    /// Tests for the <see cref="ApiHeaderParameter"/> class.
    /// </summary>
    public class ApiHeaderParameterTests
    {
        /// <summary>
        /// Tests that the constructor of <see cref="ApiHeaderParameter"/> sets the Location property to ParameterLocation.Header.
        /// </summary>
        [Fact]
        public void ApiHeaderParameter_Constructor_SetsLocationToHeader()
        {
            // Arrange & Act
            // Use Activator to bypass 'required' member compile-time enforcement so we can test
            // the constructor behavior that sets Location to Header.
            var parameter = Activator.CreateInstance<ApiHeaderParameter>()!;

            // Assert
            Assert.Equal(ParameterLocation.Header, parameter.Location);
        }

        /// <summary>
        /// Tests that the object initializer can override the Location property set by the constructor of <see cref="ApiHeaderParameter"/>.
        /// </summary>
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
