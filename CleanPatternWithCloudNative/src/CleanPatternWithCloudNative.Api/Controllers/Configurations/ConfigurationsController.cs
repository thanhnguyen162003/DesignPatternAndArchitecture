using CleanPatternWithCloudNative.Api.Models.Common;
using CleanPatternWithCloudNative.Application.Configurations.Queries;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CleanPatternWithCloudNative.Api.Controllers.Configurations
{
    [Route("api/configurations")]
    [ApiController]
    public class ConfigurationsController(ISender sender) : ControllerBase
    {
        [HttpGet("read")]
        public async Task<ActionResult<Product>> Read([FromQuery] string name)
        {
            var command = new ReadConfigurationQuery(name);
            var result = await sender.Send(command);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
