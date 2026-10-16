using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiForge.Infrastructure.Parser
{
    /// <summary>
    /// Partial class for OpenApiParser that contains logging methods
    /// </summary>
    /// <param name="logger"></param>
    public partial class OpenApiParser(ILogger<OpenApiParser>? logger = null) : IOpenApiParser
    {
        private readonly ILogger<OpenApiParser> _logger = logger ?? NullLogger<OpenApiParser>.Instance;

        // ... en ParseAsync, tras comprobar que result.Document no es null:
        //     var definition = MapDefinition(result.Document);
        //     LogParsed(definition.Title, definition.Version, definition.Endpoints.Count,
        //               definition.Models.Count, definition.Architecture);
        //     return definition;

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Parsed OpenAPI '{title}' v{version}: {endpointCount} endpoints, {modelCount} models, architecture {architecture}")]
        private partial void LogParsed(string title, string version, int endpointCount, int modelCount, ArchitectureStyle architecture);
    }
}
