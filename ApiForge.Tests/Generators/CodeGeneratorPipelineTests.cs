using ApiForge.Domain.Enums;
using ApiForge.Domain.Models;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Generator;

namespace ApiForge.Tests.Generators
{
    public class CodeGeneratorPipelineTests
    {
        public static TheoryData<ArchitectureStyle, string, string, string> Layouts => new()
        {
            {
                ArchitectureStyle.Clean,
                "Shop.Application/Interfaces/IPetsClient.cs",
                "Shop.Infrastructure/Clients/PetsClient.cs",
                "Shop.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs"
            },
            {
                ArchitectureStyle.Hexagonal,
                "Shop.Application/Ports/Driven/IPetsClientPort.cs",
                "Shop.Infrastructure/Adapters/Driven/PetsClientAdapter.cs",
                "Shop.Infrastructure/Adapters/DependencyInjection/ServiceCollectionExtensions.cs"
            }
        };

        [Theory]
        [MemberData(nameof(Layouts))]
        public async Task GenerateAsync_ProducesExpectedLayout(
            ArchitectureStyle style, string contractPath, string clientPath, string diPath)
        {
            var definition = new ApiDefinition
            {
                Title = "Shop",
                Version = "1",
                Architecture = style,
                Models = [new ApiModel { Name = "Pet", Properties = [new ApiProperty { Name = "Id", Type = "int" }] }],
                Endpoints =
                [
                    new ApiEndpoint
                    {
                        Route = "/pets",
                        HttpMethod = "GET",
                        OperationId = "listPets",
                        Response = new ArraySchema
                        {
                            OpenApiType = "array",
                            ClrType = "array",
                            ItemSchema = new ReferenceSchema { ReferenceName = "Pet", OpenApiType = "object", ClrType = "Pet" }
                        }
                    }
                ]
            };

            var solution = await new CodeGenerator().GenerateAsync(definition);
            var paths = solution.Files.Select(f => f.RelativePath).ToList();

            Assert.Equal("Shop", solution.Name);
            Assert.Contains("Shop.sln", paths);
            Assert.Contains("Shop.Domain/Shop.Domain.csproj", paths);
            Assert.Contains("Shop.Domain/Models/Pet.cs", paths);
            Assert.Contains(contractPath, paths);
            Assert.Contains(clientPath, paths);
            Assert.Contains(diPath, paths);
            Assert.Contains("Shop.Client/Program.cs", paths);
        }

        [Fact]
        public async Task GenerateAsync_NullDefinition_Throws() =>
            await Assert.ThrowsAsync<ArgumentNullException>(() => new CodeGenerator().GenerateAsync(null!));

        [Fact]
        public void GenerateSolutionFile_ListsFourProjects()
        {
            var ns = Infrastructure.Generator.Planning.ProjectNamespaces.From(
                "Shop", Infrastructure.Generator.Planning.ArchitectureConventions.For(ArchitectureStyle.Clean));

            var file = ProjectFileGenerator.GenerateSolutionFile(ns);

            Assert.Equal("Shop.sln", file.RelativePath);
            Assert.Equal(4, file.Content.Split("EndProject").Length - 1);
        }
    }
}