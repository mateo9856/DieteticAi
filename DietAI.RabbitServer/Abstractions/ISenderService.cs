using RabbitMQ.Client;

namespace DietAI.RabbitServer.Abstractions;

public interface ISenderService
{
    Task SendToQueueAsync(string queueName, string message, bool persistent = false);

    Task SendToQueueAsync(string queueName, byte[] message, bool persistent = false);

    Task SendToQueueAsync<T>(string queueName, T message, bool persistent = false);

    Task PublishToExchangeAsync(string exchangeName, string routingKey, string message, bool persistent = false);

    Task PublishToExchangeAsync(string exchangeName, string routingKey, byte[] message, bool persistent = false);

    Task PublishToExchangeAsync<T>(string exchangeName, string routingKey, T message, bool persistent = false);

    Task PublishWithPropertiesAsync(string exchangeName, string routingKey, byte[] message, IBasicProperties properties);
}