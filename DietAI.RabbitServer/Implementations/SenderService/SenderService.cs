using System.Text;
using DietAI.RabbitServer.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.SenderService;

public class SenderService : ISenderService
{
    private readonly IChannel _channel;

    public SenderService(IChannel channel)
    {
        _channel = channel;
    }
    
    public async Task SendToQueueAsync(string queueName, string message, bool persistent = false)
    {
        await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: Encoding.UTF8.GetBytes(message), mandatory: persistent);
    }

    public async Task SendToQueueAsync(string queueName, byte[] message, bool persistent = false)
    {
        await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: message, mandatory: persistent);
    }

    public async Task SendToQueueAsync<T>(string queueName, T message, bool persistent = false)
    {
        var jsonMessage = JsonConvert.SerializeObject(message)
            ?? throw new ArgumentNullException(nameof(message), "Message serialization resulted is null");

        await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: Encoding.UTF8.GetBytes(jsonMessage), mandatory: persistent);
    }

    public async Task PublishToExchangeAsync(string exchangeName, string routingKey, string message, bool persistent = false)
    {
        await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: Encoding.UTF8.GetBytes(message), mandatory: persistent);
    }

    public async Task PublishToExchangeAsync(string exchangeName, string routingKey, byte[] message, bool persistent = false)
    {
        await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: message, mandatory: persistent);
    }

    public async Task PublishToExchangeAsync<T>(string exchangeName, string routingKey, T message, bool persistent = false)
    {
        var jsonMessage = JsonConvert.SerializeObject(message)
            ?? throw new ArgumentNullException(nameof(message), "Message serialization resulted is null");

        await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: Encoding.UTF8.GetBytes(jsonMessage), mandatory: persistent);
    }

    public async Task PublishWithPropertiesAsync(string exchangeName, string routingKey, byte[] message, IBasicProperties properties)
    {
        throw new NotImplementedException();
    }
}