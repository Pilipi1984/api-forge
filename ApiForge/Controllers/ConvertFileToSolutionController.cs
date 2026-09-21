using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Generator;
using ApiForge.Infrastructure.Helpers;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharpYaml;

namespace ApiForge.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/convert")]
    public class ConvertFileToSolutionController : ControllerBase
    {
        private readonly ApiForgeGenerator _generator;

        public ConvertFileToSolutionController(ApiForgeGenerator generator)
        {
            _generator = generator;
        }

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
        /// <param name="cancellationToken"></param>
        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(
            IFormFile? file,
            [FromForm] string? architecture,
            CancellationToken cancellationToken)
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
                return BadRequest(new { error = $"Failed to generate solution: {ex.Message}" });
            }
        }
    }
}
