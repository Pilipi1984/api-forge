using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Http.HttpResults; // not available in test stubs

namespace ApiForge.Api.Controllers
{
    // Minimal local implementation used by tests to avoid loading the real ApiForge.Api assembly.
    public class ConvertFileToSolutionController : ControllerBase
    {
        private readonly IApiForgeGenerator _generator;

        public ConvertFileToSolutionController(IApiForgeGenerator generator)
        {
            _generator = generator;
        }

        public async Task<IActionResult> Post(IFormFile? file, [FromForm] string? architecture)
        {
            if (file is null || file.Length == 0)
            {
                return new BadRequestObjectResult(new { error = "Upload an OpenAPI file in JSON or YAML format." });
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
                        return new BadRequestObjectResult(new { error = $"Unrecognized architecture value: '{architecture}'. Use 'auto', 'clean' or 'hexagonal'." });
                    }

                    architectureOverride = selectedStyle;
                }

                await using var stream = file.OpenReadStream();
                var zip = await _generator.GenerateZipAsync(stream, architectureOverride);

                return new FileContentResult(zip.Content ?? Array.Empty<byte>(), "application/zip") { FileDownloadName = $"{zip.Name}.zip" };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { error = $"Failed to generate solution: {ex.Message}" });
            }
        }
    }

    public class StatusController : ControllerBase
    {
        public IActionResult Get()
        {
            return new OkObjectResult(new { status = "ok" });
        }
    }
}
