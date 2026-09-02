using ApiForge.ApplicationCore.DTOs.Responses;
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
                Status = "OK",
                Version = typeof(StatusController).Assembly.GetName().Version?.ToString() ?? "unknown",
                Environment = HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>().EnvironmentName,
                UtcTimestamp = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - StartedAt
            };

            return Ok(response);
        }
    }
}