using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;
using Xunit;

namespace ApiForge.Tests.Generators
{
    public class ProgramGeneratorTests
    {
        [Fact]
        public void Generate_Includes_ServiceRegistration_And_Usings()
        {
            var conventions = ArchitectureConventions.For(ApiForge.Domain.Enums.ArchitectureStyle.Clean);
            var ns = ProjectNamespaces.From("Acme.Root", conventions);

            var plan = new SolutionPlan { RootNamespace = "Acme.Root", Conventions = conventions, Style = ApiForge.Domain.Enums.ArchitectureStyle.Clean, Namespaces = ns, Groups = new System.Collections.Generic.List<ClientGroupPlan>() };

            var file = ProgramGenerator.Generate(plan);

            Assert.Contains("using Microsoft.Extensions.DependencyInjection;", file.Content);
            Assert.Contains("services.", file.Content);
            Assert.Contains("Console.WriteLine", file.Content);
            Assert.EndsWith("Program.cs", file.RelativePath);
        }
    }
}
