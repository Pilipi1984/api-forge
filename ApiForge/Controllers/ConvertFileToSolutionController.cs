using ApiForge.Application.Interfaces;
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

        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { error = "Upload an OpenAPI file in JSON or YAML format." });
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var definition = await _parser.ParseAsync(stream);
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
                return File(memoryStream.ToArray(), "application/zip", $"{solution.Name}{Guid.NewGuid()}.zip");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Failed to generate solution: {ex.Message}" });
            }
        }
    }
}