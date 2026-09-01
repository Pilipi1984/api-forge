using Microsoft.AspNetCore.Mvc;

namespace ApiForge.Api.Controllers
{
    [ApiController]
    [Route("status")]
    public class StatusController : ControllerBase
    {
        private static readonly DateTimeOffset StartedAt = DateTimeOffset.UtcNow;

        [HttpGet]
        public IActionResult Get()
        {
            var response = new StatusResponse
            {
                Status = "Healthy",
                Version = typeof(StatusController).Assembly.GetName().Version?.ToString() ?? "unknown",
                Environment = HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>().EnvironmentName,
                UtcTimestamp = DateTimeOffset.UtcNow,
                Uptime = DateTimeOffset.UtcNow - StartedAt
            };

            return Ok(response);
        }
    }

    public sealed class StatusResponse
    {
        public required string Status { get; init; }
        public required string Version { get; init; }
        public required string Environment { get; init; }
        public required DateTimeOffset UtcTimestamp { get; init; }
        public required TimeSpan Uptime { get; init; }
    }
}