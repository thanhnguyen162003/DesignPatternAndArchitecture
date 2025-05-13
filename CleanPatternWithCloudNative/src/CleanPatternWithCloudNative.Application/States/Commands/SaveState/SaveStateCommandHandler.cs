using CleanPatternWithCloudNative.Domain.Abstract;

using Dapr.Client;

using MediatR;

using Microsoft.Extensions.Logging;

namespace CleanPatternWithCloudNative.Application.States.Commands.SaveState
{
    public sealed class SaveStateCommandHandler(
        DaprClient daprClient,
        ILogger<SaveStateCommandHandler> logger) : IRequestHandler<SaveStateCommand>
    {
        public async Task Handle(SaveStateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogInformation("Saving product state: {State}", request);
            await daprClient.SaveStateAsync(
                Constants.StateStoreName,
                request.Name,
                request,
                cancellationToken: cancellationToken);
        }
    }
}