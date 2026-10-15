using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Generators
{
    /// <summary>
    /// Tests for the DependencyInjectionGenerator class
    /// </summary>
    public class DependencyInjectionGeneratorTests
    {
        /// <summary>
        /// Tests that the Generate method includes HttpClient registrations and the 
        /// extension method name in the generated file.
        /// </summary>
        [Fact]
        public void Generate_IncludesHttpClientRegistrations_AndExtensionName()
        {
            var conventions = ArchitectureConventions.For(ApiForge.Domain.Enums.ArchitectureStyle.Clean);
            var ns = ProjectNamespaces.From("Acme.Root", conventions);

            var group = new ClientGroupPlan
            {
                GroupName = "Payments",
                InterfaceName = conventions.InterfaceName("Payments"),
                ClassName = conventions.ClassName("Payments"),
                Endpoints = []
            };

            var plan = new SolutionPlan
            {
                RootNamespace = "Acme.Root",
                Style = Domain.Enums.ArchitectureStyle.Clean,
                Conventions = conventions,
                Namespaces = ns,
                Groups = [group]
            };

            var file = DependencyInjectionGenerator.Generate(plan);

            Assert.NotNull(file);
            Assert.Contains("AddHttpClient<", file.Content);
            Assert.Contains(ns.ClientsExtensionMethodName, file.Content);
            Assert.EndsWith("ServiceCollectionExtensions.cs", file.RelativePath);
        }
    }
}
