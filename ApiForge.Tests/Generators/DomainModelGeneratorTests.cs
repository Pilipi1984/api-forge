using ApiForge.Infrastructure.Generator;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Generators
{
    public class DomainModelGeneratorTests
    {
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

            var conventions = Infrastructure.Generator.Planning.ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean);
            var ns = Infrastructure.Generator.Planning.ProjectNamespaces.From("Root", conventions);

            var files = DomainModelGenerator.Generate(definition, ns);

            Assert.NotNull(files);
            Assert.Contains(true, files.Select(f => f.RelativePath.Contains(modelName)));
        }

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

            var conventions = ApiForge.Infrastructure.Generator.Planning.ArchitectureConventions.For(ApiForge.Domain.Enums.ArchitectureStyle.Clean);
            var ns = ApiForge.Infrastructure.Generator.Planning.ProjectNamespaces.From("Root", conventions);

            var files = DomainModelGenerator.Generate(definition, ns);

            Assert.NotNull(files);
            var content = files.Select(f => f.Content).FirstOrDefault() ?? string.Empty;
            // Expect two properties with unique names: Id and Id1
            Assert.Contains(true, new bool[] { content.Contains(" public int Id "), content.Contains(" public int Id1 ") });
        }

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
