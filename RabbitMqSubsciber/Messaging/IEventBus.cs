namespace RabbitMqSubscriber.Messaging
{
    public interface IEventBus
    {
        void Publish<T>(string queueName, T eventMessage);
        void Subscribe<T>(string queueName, Action<T> eventHandler);
    }
}
