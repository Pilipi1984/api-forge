using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Compression;

namespace ApiForge.Generator
{
    /// <summary>
    /// In-process entry point for consuming ApiForge without going through the HTTP API:
    /// parses an OpenAPI spec and generates the C# client solution directly in memory.
    /// </summary>
    /// <remarks>
    /// Creates a generator with custom implementations (useful for testing or DI).
    /// </remarks>
    public sealed partial class ApiForgeGenerator(IOpenApiParser parser,
        ICodeGenerator generator,
        ILogger<ApiForgeGenerator>? logger = null) : IApiForgeGenerator
    {
        private readonly IOpenApiParser _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        private readonly ICodeGenerator _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        private readonly ILogger<ApiForgeGenerator> _logger = logger ?? NullLogger<ApiForgeGenerator>.Instance;

        /// <summary>
        /// Creates a generator using the default implementations of parser and code generator.
        /// </summary>
        public ApiForgeGenerator() : this(new OpenApiParser(), new CodeGenerator())
        {
        }

        /// <summary>
        /// Parses the given OpenAPI spec and generates the solution files in memory.
        /// </summary>
        /// <param name="openApiSpec">Stream with an OpenAPI 3.0/3.1 document (JSON or YAML).</param>
        /// <param name="architectureOverride">Forces Clean or Hexagonal architecture, ignoring what the spec declares/suggests.</param>
        public async Task<GeneratedSolution> GenerateAsync(
            Stream openApiSpec,
            ArchitectureStyle? architectureOverride = null)
        {
            ArgumentNullException.ThrowIfNull(openApiSpec);

            var definition = await _parser.ParseAsync(openApiSpec);
            if (architectureOverride is not null)
            {
                definition.Architecture = architectureOverride.Value;
            }

            LogArchitectureOverride(definition.Architecture, architectureOverride ?? definition.Architecture);
            return await _generator.GenerateAsync(definition);
        }

        /// <summary>
        /// Same as <see cref="GenerateAsync"/> but returns the solution packaged as a .zip file,
        /// equivalent to what the <c>POST /v1/convert</c> endpoint returns.
        /// </summary>
        public async Task<GeneratedZipArchive> GenerateZipAsync(
             Stream openApiSpec,
             ArchitectureStyle? architectureOverride = null)
        {
            var solution = await GenerateAsync(openApiSpec, architectureOverride);
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in solution.Files)
                {
                    var entry = archive.CreateEntry(file.RelativePath, CompressionLevel.Optimal);
                    await using var entryStream = await entry.OpenAsync();
                    await using var writer = new StreamWriter(entryStream);
                    await writer.WriteAsync(file.Content);
                }
            }

            LogZipCreated(solution.Name, solution.Files.Count, memoryStream.Length);
            return new GeneratedZipArchive
            {
                Name = solution.Name,
                Content = memoryStream.ToArray()
            };
        }

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Architecture override applied: {detected} -> {selected}")]
        private partial void LogArchitectureOverride(ArchitectureStyle detected, ArchitectureStyle selected);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Zip created for {Solution}: {fileCount} files, {sizeBytes} bytes")]
        private partial void LogZipCreated(string solution, int fileCount, long sizeBytes);
    }
}