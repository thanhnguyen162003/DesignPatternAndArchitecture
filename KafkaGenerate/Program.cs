using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace KafkaTopicCreator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bootstrapServers = "";
            var saslUsername = "";
            var saslPassword = "";

            var config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers,
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = saslUsername,
                SaslPassword = saslPassword
            };

            var topicsWithThreePartitions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "topic"
            };

            var topicsWithOneDayRetention = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "topic"
            };

            var allTopics = new List<string>
            {
                "topic"
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet(StringComparer.OrdinalIgnoreCase);

                var topicsToCreate = allTopics
                    .Where(topic => !existingTopics.Contains(topic))
                    .Select(topic => new TopicSpecification
                    {
                        Name = topic,
                        NumPartitions = topicsWithThreePartitions.Contains(topic) ? 3 : 1,
                        ReplicationFactor = 3,
                        Configs = topicsWithOneDayRetention.Contains(topic)
                            ? new Dictionary<string, string> { { "retention.ms", "86400000" } }
                            : null
                    })
                    .ToList();

                if (topicsToCreate.Count == 0)
                {
                    Console.WriteLine("✅ All topics already exist. Nothing to create.");
                    return;
                }

                await adminClient.CreateTopicsAsync(topicsToCreate);
                Console.WriteLine($"✅ Successfully created {topicsToCreate.Count} topics.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }
    }
}