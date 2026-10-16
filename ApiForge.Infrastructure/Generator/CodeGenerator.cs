using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates a complete C# solution based on the provided API definition, including domain models, 
    /// API contracts, client implementations, dependency injection setup, and project files.
    /// </summary>
    public partial class CodeGenerator(ILogger<CodeGenerator>? logger = null) : ICodeGenerator
    {
        /// <summary>
        /// The logger instance used for logging within the CodeGenerator class. If no logger is provided,
        /// a NullLogger instance is used to avoid null reference exceptions.
        /// </summary>
        private readonly ILogger<CodeGenerator> _logger = logger ?? NullLogger<CodeGenerator>.Instance;

        /// <summary>
        /// Generates a complete C# solution based on the provided API definition.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns>Returns the generated solution.</returns>
        public Task<GeneratedSolution> GenerateAsync(ApiDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            var rootNamespace = NameHelper.ToSolutionName(definition.Title);
            var plan = SolutionPlanner.CreatePlan(definition, rootNamespace);
            LogPlanCreated(rootNamespace, plan.Style, plan.Groups.Count);
            var ns = plan.Namespaces;

            var files = new List<GeneratedFile>();
            files.AddRange(ProjectFileGenerator.GenerateProjectFiles(ns));
            files.Add(ProjectFileGenerator.GenerateSolutionFile(ns));
            files.AddRange(DomainModelGenerator.Generate(definition, ns));
            files.AddRange(ApiContractGenerator.Generate(plan));
            files.AddRange(ApiClientImplementationGenerator.Generate(plan));
            files.Add(DependencyInjectionGenerator.Generate(plan));
            files.Add(ProgramGenerator.Generate(plan));

            var solution = new GeneratedSolution
            {
                Name = rootNamespace,
                RootNamespace = rootNamespace,
                Files = files
            };

            LogGenerated(rootNamespace, files.Count);
            return Task.FromResult(solution);
        }


        [LoggerMessage(Level = LogLevel.Information,
            Message = "Planning solution {solution} with style {style}: {groupCount} client groups")]
        private partial void LogPlanCreated(string solution, ArchitectureStyle style, int groupCount);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Generated solution {solution}: {fileCount} files")]
        private partial void LogGenerated(string solution, int fileCount);
    }
}
