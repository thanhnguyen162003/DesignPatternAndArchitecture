using CleanPatternWithCloudNative.Domain.Abstract;

using Dapr.Client;

using MediatR;

using Microsoft.Extensions.Logging;

namespace CleanPatternWithCloudNative.Application.Configurations.Queries
{
    public class ReadConfigurationQueryHandler(
        DaprClient daprClient,
        ILogger<ReadConfigurationQueryHandler> logger) : IRequestHandler<ReadConfigurationQuery, string?>
    {
        public async Task<string?> Handle(
            ReadConfigurationQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            GetConfigurationResponse response = await daprClient.GetConfiguration(
                Constants.ConfigStoreName,
                [request.Key],
                null,
                cancellationToken);

            if (!response.Items.TryGetValue(request.Key, out var configItem))
            {
                logger.LogError("Config not found for Key: {Key}", request.Key);
                return null;
            }
            logger.LogInformation("Config found for {Key}: {Value}", request.Key, configItem.Value);
            return configItem.Value;
        }
    }
}
