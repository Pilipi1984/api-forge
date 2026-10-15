using ApiForge.Infrastructure.Generator;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Generators
{
    /// <summary>
    /// Tests for the DomainModelGenerator class
    /// </summary>
    public class DomainModelGeneratorTests
    {
        /// <summary>
        /// Tests that the Generate method creates files for the specified models in the API definition
        /// </summary>
        /// <param name="modelName"></param>
        [Theory]
        [InlineData("Person")]
        [InlineData("Order")]
        public void Generate_CreatesFiles_ForModels(string modelName)
        {
            var definition = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Models =
                [
                    new() { Name = modelName, Properties = [new() { Name = "Id", Type = "int" }] }
                ]
            };

            var conventions = ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean);
            var ns = ProjectNamespaces.From("Root", conventions);

            var files = DomainModelGenerator.Generate(definition, ns);

            Assert.NotNull(files);
            Assert.Contains(true, files.Select(f => f.RelativePath.Contains(modelName)));
        }

        /// <summary>
        /// Tests that the Generate method handles duplicate property names by renaming them to unique names
        /// </summary>
        [Fact]
        public void Generate_Handles_DuplicatePropertyNames()
        {
            var definition = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Models =
                [
                    new ApiModel
                    {
                        Name = "Person",
                        Properties =
                        [
                            new ApiProperty { Name = "Id", Type = "Int32" },
                            new ApiProperty { Name = "Id", Type = "Int32" }
                        ]
                    }
                ]
            };

            var conventions = ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean);
            var ns = ProjectNamespaces.From("Root", conventions);

            var files = DomainModelGenerator.Generate(definition, ns);

            Assert.NotNull(files);
            var content = files.Select(f => f.Content).FirstOrDefault() ?? string.Empty;
            // Expect two properties with unique names: Id and Id1
            Assert.Contains(true, new bool[] { content.Contains(" public int Id "), content.Contains(" public int Id1 ") });
        }

        /// <summary>
        /// Tests that the Generate method includes default assignment for string properties in the generated model class
        /// </summary>
        [Fact]
        public void Generate_Includes_DefaultAssignment_ForString()
        {
            var definition = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Models =
                [
                    new ApiModel
                    {
                        Name = "Person",
                        Properties =
                        [
                            new ApiProperty { Name = "Name", Type = "String" }
                        ]
                    }
                ]
            };

            ArchitectureConventions conventions = ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean);
            ProjectNamespaces ns = ProjectNamespaces.From("Root", conventions);

            var files = DomainModelGenerator.Generate(definition, ns);

            Assert.NotNull(files);
            var content = files.Select(f => f.Content).FirstOrDefault() ?? string.Empty;
            Assert.Contains("= string.Empty;", content);
        }
    }
}
