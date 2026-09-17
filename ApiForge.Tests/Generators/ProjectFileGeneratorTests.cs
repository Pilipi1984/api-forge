using ApiForge.Infrastructure.Generator;

namespace ApiForge.Tests.Generators
{
    public class ProjectFileGeneratorTests
    {
        private static readonly object[] RootsAndStyles =
        {
            new object[] { "TestRoot", ApiForge.Domain.Enums.ArchitectureStyle.CleanArchitecture },
            new object[] { "Acme.App", ApiForge.Domain.Enums.ArchitectureStyle.Hexagonal }
        };

        [Test]
        [TestCaseSource(nameof(RootsAndStyles))]
        public void GenerateProjectFiles_ReturnsCoreFiles(string rootNamespace, ApiForge.Domain.Enums.ArchitectureStyle style)
        {
            var conventions = ApiForge.Infrastructure.Generator.Planning.ArchitectureConventions.For(style);
            var ns = ApiForge.Infrastructure.Generator.Planning.ProjectNamespaces.From(rootNamespace, conventions);

            var files = ProjectFileGenerator.GenerateProjectFiles(ns);

            Assert.IsNotNull(files);
            Assert.IsTrue(files.Any(f => f.RelativePath.EndsWith(".csproj")), "Expected at least one .csproj file");
            // Ensure csproj contains the target framework for each generated project
            Assert.IsTrue(files.All(f => f.Content.Contains("<TargetFramework>net10.0</TargetFramework>")));
        }
    }
}
