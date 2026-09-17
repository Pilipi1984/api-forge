using ApiForge.Infrastructure.Generator;
using ApiForge.Domain.Models;
using NUnit.Framework;
using System.Linq;

namespace ApiForge.Tests.Generators
{
    public class DomainModelGeneratorTests
    {
        [Test]
        [TestCase("Person")]
        [TestCase("Order")]
        public void Generate_CreatesFiles_ForModels(string modelName)
        {
            var definition = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Models = new System.Collections.Generic.List<ApiModel>
                {
                    new ApiModel { Name = modelName, Properties = new System.Collections.Generic.List<ApiProperty> { new ApiProperty { Name = "Id", Type = "int" } } }
                }
            };

            var conventions = ApiForge.Infrastructure.Generator.Planning.ArchitectureConventions.For(ApiForge.Domain.Enums.ArchitectureStyle.CleanArchitecture);
            var ns = ApiForge.Infrastructure.Generator.Planning.ProjectNamespaces.From("Root", conventions);

            var files = DomainModelGenerator.Generate(definition, ns);

            Assert.IsNotNull(files);
            Assert.IsTrue(files.Any(f => f.RelativePath.Contains(modelName)), $"Expected generated files to include model name {modelName}");
        }
    }
}
