using CleanPatternWithCloudNative.Api.Models.Common;
using CleanPatternWithCloudNative.Application.PubSub.Commands.PublishProduct;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CleanPatternWithCloudNative.Api.Controllers.Publisher
{
    [Route("api/publisher")]
    [ApiController]
    public class PublisherController(
        ISender sender,
        ILogger<PublisherController> logger) : ControllerBase
    {
        [HttpPost("publish")]
        public async Task<IActionResult> Publish(
            [FromBody] Product message,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);

            logger.LogInformation("Publishing message: {@Message}", message);
            var command = new PublishProductCommand(message.Name, message.Description);
            await sender.Send(command, cancellationToken);

            return Ok();
        }
    }
}