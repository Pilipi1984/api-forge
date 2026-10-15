using Microsoft.OpenApi;
using ApiForge.Domain.Models.ApiParameters;

namespace ApiForge.Tests.Models.ApiParameters
{
    /// <summary>
    /// Unit tests for the <see cref="ApiCookieParameter"/> class.
    /// </summary>
    public class ApiCookieParameterTests
    {
        /// <summary>
        /// Tests that the constructor of <see cref="ApiCookieParameter"/> 
        /// sets the <see cref="ApiParameter.Location"/> property to <see cref="ParameterLocation.Cookie"/>.
        /// </summary>
        [Fact]
        public void Ctor_Default_SetsLocationToCookie()
        {
            // Arrange
            var name = "testName";
            var type = "string";

            // Act
            var param = new ApiCookieParameter
            {
                // required property must be set by the initializer per C# rules
                Location = ParameterLocation.Cookie,
                Name = name,
                Type = type
            };

            // Assert
            Assert.Equal(ParameterLocation.Cookie, param.Location);
            Assert.Equal(name, param.Name);
            Assert.Equal(type, param.Type);
            Assert.False(param.Required);
            Assert.Null(param.Description);
        }

        /// <summary>
        /// Tests that the constructor of <see cref="ApiCookieParameter"/>
        /// </summary>
        [Fact]
        public void Ctor_WhenInitializerProvidesLocation_InitializerOverridesConstructor()
        {
            // Arrange
            var name = "another";
            var type = "int";

            // Act
            var param = new ApiCookieParameter
            {
                // initializer should run after constructor and override the value set in ctor
                Location = ParameterLocation.Header,
                Name = name,
                Type = type
            };

            // Assert
            Assert.Equal(ParameterLocation.Header, param.Location);
            Assert.Equal(name, param.Name);
            Assert.Equal(type, param.Type);
        }
    }
}
