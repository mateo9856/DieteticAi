using DietAI.RabbitServer.Abstractions;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.SenderService;

public class SenderService : ISenderService
{
    public async Task SendToQueueAsync(string queueName, string message, bool persistent = false)
    {
        throw new NotImplementedException();
    }

    public async Task SendToQueueAsync(string queueName, byte[] message, bool persistent = false)
    {
        throw new NotImplementedException();
    }

    public async Task SendToQueueAsync<T>(string queueName, T message, bool persistent = false)
    {
        throw new NotImplementedException();
    }

    public async Task PublishToExchangeAsync(string exchangeName, string routingKey, string message, bool persistent = false)
    {
        throw new NotImplementedException();
    }

    public async Task PublishToExchangeAsync(string exchangeName, string routingKey, byte[] message, bool persistent = false)
    {
        throw new NotImplementedException();
    }

    public async Task PublishToExchangeAsync<T>(string exchangeName, string routingKey, T message, bool persistent = false)
    {
        throw new NotImplementedException();
    }

    public async Task PublishWithPropertiesAsync(string exchangeName, string routingKey, byte[] message, IBasicProperties properties)
    {
        throw new NotImplementedException();
    }
}