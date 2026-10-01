using ApiForge.Infrastructure.Generator;
using ApiForge.Domain.Models;

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
                    new() { Name = modelName, Properties = new System.Collections.Generic.List<ApiProperty> { new ApiProperty { Name = "Id", Type = "int" } } }
                ]
            };

            var conventions = Infrastructure.Generator.Planning.ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean);
            var ns = Infrastructure.Generator.Planning.ProjectNamespaces.From("Root", conventions);

            var files = DomainModelGenerator.Generate(definition, ns);

            Assert.NotNull(files);
            Assert.True(files.Any(f => f.RelativePath.Contains(modelName)), $"Expected generated files to include model name {modelName}");
        }
    }
}
