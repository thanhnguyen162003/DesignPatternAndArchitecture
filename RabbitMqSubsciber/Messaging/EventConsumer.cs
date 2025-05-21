using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqSubscriber.Common.Models;
using System.Text;

namespace RabbitMqSubsciber.Messaging
{
    public class EventConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public EventConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "order_exchange", type: "fanout");
            _channel.QueueDeclare(queue: "inventory_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: "inventory_queue", exchange: "order_exchange", routingKey: "");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var orderCreatedEvent = JsonConvert.DeserializeObject<OrderCreatedEvent>(message);

                // Logic to adjust inventory
                Console.WriteLine($"[InventoryService] Received OrderCreatedEvent for OrderId: {orderCreatedEvent.OrderId}");
            };

            _channel.BasicConsume(queue: "inventory_queue", autoAck: true, consumer: consumer);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
