using ApiForge.Domain.Enums;
using ApiForge.Infrastructure.Generator;

namespace ApiForge.Tests.Generators
{
    public class ProjectFileGeneratorTests
    {
        public static TheoryData<string, ArchitectureStyle> RootsAndStyles() => new()
        {
            { "TestRoot", ArchitectureStyle.Clean },
            { "Acme.App", ArchitectureStyle.Hexagonal }
        };

        [Theory]
        [MemberData(nameof(RootsAndStyles))]
        public void GenerateProjectFiles_ReturnsCoreFiles(string rootNamespace, Domain.Enums.ArchitectureStyle style)
        {
            var conventions = Infrastructure.Generator.Planning.ArchitectureConventions.For(style);
            var ns = Infrastructure.Generator.Planning.ProjectNamespaces.From(rootNamespace, conventions);

            var files = ProjectFileGenerator.GenerateProjectFiles(ns);

            Assert.NotNull(files);
            Assert.Contains(true, files.Select(f => f.RelativePath.EndsWith(".csproj")));
            // Ensure csproj contains the target framework for each generated project
            Assert.DoesNotContain(false, files.Select(f => f.Content.Contains("<TargetFramework>net10.0</TargetFramework>")));
        }
    }
}
