using CleanPatternWithCloudNative.Domain.Abstract;

using Dapr.Client;

using MediatR;

using Microsoft.Extensions.Logging;

namespace CleanPatternWithCloudNative.Application.States.Queries.ReadState
{
    public sealed class ReadStateQueryHandler(
        DaprClient daprClient,
        ILogger<ReadStateQueryHandler> logger) : IRequestHandler<ReadStateQuery, ProductState?>
    {
        public async Task<ProductState?> Handle(ReadStateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            ProductState? state = await daprClient.GetStateAsync<ProductState?>(
                Constants.StateStoreName,
                request.ProductName,
                cancellationToken: cancellationToken);

            logger.LogInformation("State: {State}", state);

            return state;
        }
    }
}