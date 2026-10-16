using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Infrastructure.Helpers;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiForge.Api.Controllers
{
    /// <summary>
    /// Controller for converting an uploaded OpenAPI spec into a generated .NET solution.
    /// </summary>
    /// <param name="generator">The API Forge generator.</param>
    /// <param name="logger">The logger.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/convert")]
    public partial class ConvertFileToSolutionController(IApiForgeGenerator generator,
    ILogger<ConvertFileToSolutionController>? logger = null) : ControllerBase
    {
        private const long MaxSpecSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly IApiForgeGenerator _generator = generator;
        private readonly ILogger<ConvertFileToSolutionController> _logger =
            logger ?? NullLogger<ConvertFileToSolutionController>.Instance;


        /// <summary>
        /// Converts an uploaded OpenAPI spec into a generated .NET solution, zipped for download.
        /// </summary>
        /// <param name="file">The OpenAPI spec file (JSON or YAML).</param>
        /// <param name="architecture">
        /// Optional explicit architecture override coming from the UI selector:
        /// "auto" (or omitted) keeps whatever <see cref="IOpenApiParser"/> detected from the spec
        /// ("x-architecture" extension, or the tag-name heuristic as fallback); "clean" or
        /// "hexagonal" forces that style regardless of what the spec says.
        /// </param>
        [HttpPost]
        [RequestSizeLimit(MaxSpecSizeBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxSpecSizeBytes)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(
            IFormFile? file,
            [FromForm] string? architecture)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { error = "Upload an OpenAPI file in JSON or YAML format." });
            }

            try
            {
                ArchitectureStyle? architectureOverride = null;
                var isExplicitOverride = !string.IsNullOrWhiteSpace(architecture) &&
                                    !string.Equals(architecture, "auto", StringComparison.OrdinalIgnoreCase);

                if (isExplicitOverride)
                {
                    if (!ArchitectureStyleParser.TryParse(architecture, out var selectedStyle))
                    {
                        LogInvalidArchitecture(architecture);
                        return BadRequest(new { error = $"Unrecognized architecture value: '{architecture}'. Use 'auto', 'clean' or 'hexagonal'." });
                    }

                    architectureOverride = selectedStyle;
                }

                await using var stream = file.OpenReadStream();
                var zip = await _generator.GenerateZipAsync(stream, architectureOverride);

                return File(zip.Content, "application/zip", $"{zip.Name}.zip");
            }
            catch (Exception ex)
            {
                LogConversionFailed(ex, file.FileName, file.Length);
                return BadRequest(new { error = $"Failed to generate solution: {ex.Message}" });
            }
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = "Convert request without a file")]
        private partial void LogMissingFile();

        [LoggerMessage(Level = LogLevel.Warning, Message = "Unrecognized architecture value '{architecture}'")]
        private partial void LogInvalidArchitecture(string? architecture);

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to generate solution from '{fileName}' ({length} bytes)")]
        private partial void LogConversionFailed(Exception ex, string fileName, long length);
    }
}
