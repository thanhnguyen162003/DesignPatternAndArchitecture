using CleanPatternWithCloudNative.Domain.Abstract;

using Dapr.Client;

using MediatR;

using Microsoft.Extensions.Logging;

namespace CleanPatternWithCloudNative.Application.PubSub.Commands.PublishProduct
{
    public sealed class PublishProductCommandHandler(
        DaprClient daprClient,
        ILogger<PublishProductCommandHandler> logger) : IRequestHandler<PublishProductCommand>
    {
        public async Task Handle(
            PublishProductCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Publishing message: {Message}", request);
            await daprClient.PublishEventAsync(
                Constants.PubSubName,
                Constants.PubSubTopicName,
                request,
                cancellationToken);
        }
    }
}