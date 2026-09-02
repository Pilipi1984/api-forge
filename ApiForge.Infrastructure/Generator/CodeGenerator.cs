using ApiForge.Application.Interfaces;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator
{
    public class CodeGenerator : ICodeGenerator
    {
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
