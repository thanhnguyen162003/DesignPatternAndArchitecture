using System.Diagnostics.Metrics;

using Microsoft.AspNetCore.Mvc;

namespace CleanPatternWithCloudNative.Api.Controllers.Bindings
{
    [Route("api/bindings")]
    [ApiController]
    public class BindingsController(
        ILogger<BindingsController> logger,
        Counter<int> counter) : ControllerBase
    {
        [HttpPost("cron")]
        public IActionResult Cron()
        {
            counter.Add(1);

            logger.LogInformation("Cron job executed");
            return Ok();
        }
    }
}