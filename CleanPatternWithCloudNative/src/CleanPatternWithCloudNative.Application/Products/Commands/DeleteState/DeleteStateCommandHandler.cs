using CleanPatternWithCloudNative.Domain.Abstract;

using Dapr.Client;

using MediatR;

using Microsoft.Extensions.Logging;

namespace CleanPatternWithCloudNative.Application.Products.Commands.DeleteState
{
    public sealed class DeleteStateCommandHandler(
        DaprClient daprClient,
        ILogger<DeleteStateCommandHandler> logger) : IRequestHandler<DeleteStateCommand>
    {
        public async Task Handle(DeleteStateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            logger.LogInformation("Deleting state: {ProductName}", request.ProductName);

            await daprClient.DeleteStateAsync(
                Constants.StateStoreName,
                request.ProductName,
                cancellationToken: cancellationToken);
        }
    }
}