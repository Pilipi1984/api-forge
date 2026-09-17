using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Domain.GeneratedApiSolution;
using NUnit.Framework;
using System.Collections.Generic;

namespace ApiForge.Tests.Generators
{
    public class DependencyInjectionGeneratorTests
    {
        [Test]
        public void Generate_IncludesHttpClientRegistrations_AndExtensionName()
        {
            var conventions = ArchitectureConventions.For(ApiForge.Domain.Enums.ArchitectureStyle.CleanArchitecture);
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
                Style = ApiForge.Domain.Enums.ArchitectureStyle.CleanArchitecture,
                Conventions = conventions,
                Namespaces = ns,
                Groups = new List<ClientGroupPlan> { group }
            };

            var file = DependencyInjectionGenerator.Generate(plan);

            Assert.IsNotNull(file);
            Assert.IsTrue(file.Content.Contains("AddHttpClient<"));
            Assert.IsTrue(file.Content.Contains(ns.ClientsExtensionMethodName));
            Assert.IsTrue(file.RelativePath.EndsWith("ServiceCollectionExtensions.cs"));
        }
    }
}
