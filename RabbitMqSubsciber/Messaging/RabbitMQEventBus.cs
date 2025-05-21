using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RabbitMqSubscriber.Messaging
{
    public class RabbitMQEventBus : IEventBus
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQEventBus()
        {
            var factory = new ConnectionFactory() { HostName = "messaging" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish<T>(string queueName, T eventMessage)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var message = JsonSerializer.Serialize(eventMessage);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }

        public void Subscribe<T>(string queueName, Action<T> eventHandler)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var eventMessage = JsonSerializer.Deserialize<T>(message);
                eventHandler(eventMessage);
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
