using KafkaConsumer.Common;
using KafkaConsumer.Common.Models;
using Newtonsoft.Json;

namespace KafkaConsumer.Consumers
{
    public class DataConsumer(
    IConfiguration configuration,
    ILogger<DataConsumer> logger,
    IServiceProvider serviceProvider)
    : KafkaConsumerDefault<DataModelProduce>(configuration, logger, serviceProvider,
        "test-topic", "consumer-group")
    {
        protected override Task ProcessMessage(string message, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DataConsumer>>();

            try
            {
                var dataModel = JsonConvert.DeserializeObject<DataModelProduce>(message);
                if (dataModel == null)
                {
                    logger.LogWarning("Received an empty or invalid message.");
                    return Task.CompletedTask;
                }
                logger.LogInformation($"consumer have consume the message {dataModel.Id}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message.");
            }

            return Task.CompletedTask;
        }
    }
}
