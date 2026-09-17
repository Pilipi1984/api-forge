using ApiForge.Application.Interfaces;
using ApiForge.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace ApiForge.Api.Controllers
{
    [ApiController]
    [Route("convert")]
    public class ConvertFileToSolutionController : ControllerBase
    {
        private readonly IOpenApiParser _parser;
        private readonly ICodeGenerator _generator;

        public ConvertFileToSolutionController(IOpenApiParser parser, ICodeGenerator generator)
        {
            _parser = parser;
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
                await using var stream = file.OpenReadStream();
                var definition = await _parser.ParseAsync(stream);

                // "auto" (or no selection) keeps the architecture detected from the spec itself.
                // Any other recognized value explicitly overrides it.
                var isExplicitOverride = !string.IsNullOrWhiteSpace(architecture) &&
                    !string.Equals(architecture, "auto", StringComparison.OrdinalIgnoreCase);

                if (isExplicitOverride)
                {
                    if (!ArchitectureStyleParser.TryParse(architecture, out var selectedStyle))
                    {
                        return BadRequest(new { error = $"Unrecognized architecture value: '{architecture}'. Use 'auto', 'clean' or 'hexagonal'." });
                    }

                    definition.Architecture = selectedStyle;
                }

                var solution = await _generator.GenerateAsync(definition);

                using var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var generatedFile in solution.Files)
                    {
                        var entry = archive.CreateEntry(generatedFile.RelativePath, CompressionLevel.Optimal);
                        await using var entryStream = entry.Open();
                        await using var writer = new StreamWriter(entryStream);
                        await writer.WriteAsync(generatedFile.Content);
                    }
                }

                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/zip", $"{solution.Name}.zip");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Failed to generate solution: {ex.Message}" });
            }
        }
    }
}
