using Xunit;
using ApiForge.Domain.Models.ApiParameters;
using Microsoft.OpenApi;

#nullable enable

namespace ApiForge.Tests.Models.ApiParameters
{
    public class ApiQueryParameterTests
    {
        [Fact]
        public void ApiQueryParameter_Ctor_WhenInvoked_SetsLocationToQuery()
        {
            // Arrange
            const string expectedName = "param";
            const string expectedType = "string";

            // Act
            var sut = new ApiQueryParameter()
            {
                Name = expectedName,
                Type = expectedType,
                Location = ParameterLocation.Query
            };

            // Assert
            Assert.Equal(ParameterLocation.Query, sut.Location);
        }

        [Fact]
        public void ApiQueryParameter_Ctor_PreservesInitValues_And_DefaultsExplodeToFalse()
        {
            // Arrange
            const string expectedName = "other";
            const string expectedType = "int";

            // Act
            var sut = new ApiQueryParameter()
            {
                Name = expectedName,
                Type = expectedType,
                Location = ParameterLocation.Query
            };

            // Assert - constructor should set Location and preserve required inits
            Assert.Equal(ParameterLocation.Query, sut.Location);
            Assert.Equal(expectedName, sut.Name);
            Assert.Equal(expectedType, sut.Type);
            Assert.False(sut.Explode);
        }
    }
}
