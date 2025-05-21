namespace RabbitMqSubscriber.Common.Models
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public required string Product { get; set; }
        public int Quantity { get; set; }
    }
}
