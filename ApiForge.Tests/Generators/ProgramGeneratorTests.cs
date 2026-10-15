using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Generators
{
    /// <summary>
    /// Tests for the ProgramGenerator class
    /// </summary>
    public class ProgramGeneratorTests
    {
        /// <summary>
        /// Tests that the Generate method includes service registration and necessary using statements in the generated Program.cs file.
        /// </summary>
        [Fact]
        public void Generate_Includes_ServiceRegistration_And_Usings()
        {
            var conventions = ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean);
            var ns = ProjectNamespaces.From("Acme.Root", conventions);

            var plan = new SolutionPlan { RootNamespace = "Acme.Root", Conventions = conventions, Style = Domain.Enums.ArchitectureStyle.Clean, Namespaces = ns, Groups = [] };

            var file = ProgramGenerator.Generate(plan);

            Assert.Contains("using Microsoft.Extensions.DependencyInjection;", file.Content);
            Assert.Contains("services.", file.Content);
            Assert.Contains("Console.WriteLine", file.Content);
            Assert.EndsWith("Program.cs", file.RelativePath);
        }
    }
}
