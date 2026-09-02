using ApiForge.Application.Interfaces;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates a complete C# solution based on the provided API definition, including domain models, 
    /// API contracts, client implementations, dependency injection setup, and project files.
    /// </summary>
    public class CodeGenerator : ICodeGenerator
    {
        /// <summary>
        /// Generates a complete C# solution based on the provided API definition.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns>Returns the generated solution.</returns>
        public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            var rootNamespace = NameHelper.ToPascalCase(
                string.IsNullOrWhiteSpace(definition.Title) ? "GeneratedApi" : definition.Title);

            var plan = SolutionPlanner.CreatePlan(definition, rootNamespace);

            var files = new List<GeneratedFile>();
            files.AddRange(ProjectFileGenerator.GenerateProjectFiles(rootNamespace));
            files.Add(ProjectFileGenerator.GenerateSolutionFile(rootNamespace));
            files.AddRange(DomainModelGenerator.Generate(definition, rootNamespace));
            files.AddRange(ApiContractGenerator.Generate(plan));
            files.AddRange(ApiClientImplementationGenerator.Generate(plan));
            files.Add(DependencyInjectionGenerator.Generate(plan));
            files.Add(ProgramGenerator.Generate(plan, rootNamespace));

            var solution = new GeneratedSolution
            {
                Name = rootNamespace,
                RootNamespace = rootNamespace,
                Files = files
            };

            return Task.FromResult(solution);
        }
    }
}
