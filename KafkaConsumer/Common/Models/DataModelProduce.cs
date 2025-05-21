namespace KafkaConsumer.Common.Models
{
    public class DataModelProduce
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Payload { get; set; }
    }
}
