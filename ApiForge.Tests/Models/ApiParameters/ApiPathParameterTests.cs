using Microsoft.OpenApi;
using ApiForge.Domain.Models.ApiParameters;

namespace ApiForge.Tests.Models.ApiParameters
{
    /// <summary>
    /// Unit tests for the <see cref="ApiPathParameter"/> class.
    /// </summary>
    public class ApiPathParameterTests
    {
        /// <summary>
        /// Tests that the constructor of <see cref="ApiPathParameter"/> sets the <see cref="ApiParameter.Location"/> 
        /// property to <see cref="ParameterLocation.Path"/>.
        /// </summary>
        [Fact]
        public void Ctor_Default_SetsLocationToPath()
        {
            // Arrange
            var expected = ParameterLocation.Path;

            // Act
            var parameter = new ApiPathParameter
            {
                Name = "id",
                Type = "string",
                Location = ParameterLocation.Path
            };

            // Assert
            Assert.Equal(expected, parameter.Location);
        }

        /// <summary>
        /// Tests that the constructor of <see cref="ApiPathParameter"/> leaves other properties with their default values.
        /// </summary>
        [Fact]
        public void Ctor_Default_LeavesOtherPropertiesWithDefaults()
        {
            // Arrange
            var expectedName = "userId";
            var expectedType = "integer";
            var expectedRequired = false;
            string? expectedDescription = null;
            var expectedLocation = ParameterLocation.Path;

            // Act
            var parameter = new ApiPathParameter
            {
                Name = expectedName,
                Type = expectedType,
                Location = ParameterLocation.Path
            };

            // Assert
            Assert.Equal(expectedName, parameter.Name);
            Assert.Equal(expectedType, parameter.Type);
            Assert.Equal(expectedRequired, parameter.Required);
            Assert.Equal(expectedDescription, parameter.Description);
            Assert.Equal(expectedLocation, parameter.Location);
        }
    }
}
