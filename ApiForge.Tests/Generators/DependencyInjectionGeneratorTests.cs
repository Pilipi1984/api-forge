using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Domain.GeneratedApiSolution;
using Xunit;
using System.Collections.Generic;

namespace ApiForge.Tests.Generators
{
    public class DependencyInjectionGeneratorTests
    {
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
                Endpoints = new List<EndpointPlan>()
            };

            var plan = new SolutionPlan
            {
                RootNamespace = "Acme.Root",
                Style = ApiForge.Domain.Enums.ArchitectureStyle.Clean,
                Conventions = conventions,
                Namespaces = ns,
                Groups = new List<ClientGroupPlan> { group }
            };

            var file = DependencyInjectionGenerator.Generate(plan);

            Assert.NotNull(file);
            Assert.Contains("AddHttpClient<", file.Content);
            Assert.Contains(ns.ClientsExtensionMethodName, file.Content);
            Assert.EndsWith("ServiceCollectionExtensions.cs", file.RelativePath);
        }
    }
}
