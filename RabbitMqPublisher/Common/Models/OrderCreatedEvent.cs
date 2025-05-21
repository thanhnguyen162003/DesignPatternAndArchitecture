namespace RabbitMqPublisher.Common.Models
{
    public class OrderCreatedEvent
    {
        public required int OrderId { get; set; }
        public required string Product { get; set; }
        public required int Quantity { get; set; }
    }
}
