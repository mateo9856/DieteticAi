using System.Text;
using DietAI.RabbitServer.Abstractions;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.SenderService;

public class SenderService : ISenderService
{
    private readonly ITopicFactory _topicFactory;

    public SenderService(ITopicFactory topicFactory)
    {
        _topicFactory = topicFactory;
    }

    public async Task SendToQueueAsync(string queueName, string message, bool persistent = false)
    {
        var channel = EnsureChannelIsActive();
        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: Encoding.UTF8.GetBytes(message), mandatory: persistent);
    }

    public async Task SendToQueueAsync(string queueName, byte[] message, bool persistent = false)
    {
        var channel = EnsureChannelIsActive();
        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: message, mandatory: persistent);
    }

    public async Task SendToQueueAsync<T>(string queueName, T message, bool persistent = false)
    {
        var channel = EnsureChannelIsActive();
        var jsonMessage = JsonConvert.SerializeObject(message)
            ?? throw new ArgumentNullException(nameof(message), "Message serialization resulted is null");

        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: Encoding.UTF8.GetBytes(jsonMessage), mandatory: persistent);
    }

    public async Task PublishToExchangeAsync(string exchangeName, string routingKey, string message, bool persistent = false)
    {
        var channel = EnsureChannelIsActive();
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: Encoding.UTF8.GetBytes(message), mandatory: persistent);
    }

    public async Task PublishToExchangeAsync(string exchangeName, string routingKey, byte[] message, bool persistent = false)
    {
        var channel = EnsureChannelIsActive();
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: message, mandatory: persistent);
    }

    public async Task PublishToExchangeAsync<T>(string exchangeName, string routingKey, T message, bool persistent = false)
    {
        var channel = EnsureChannelIsActive();
        var jsonMessage = JsonConvert.SerializeObject(message)
            ?? throw new ArgumentNullException(nameof(message), "Message serialization resulted is null");

        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: Encoding.UTF8.GetBytes(jsonMessage), mandatory: persistent);
    }

    private IChannel EnsureChannelIsActive()
    {
        var channel = _topicFactory.ActiveChannel;
        
        if (channel is null)
        {
            throw new InvalidOperationException("No active channel available from ITopicFactory.");
        }

        if (channel.IsClosed)
        {
            throw new InvalidOperationException("The channel is closed and cannot be used.");
        }

        return channel;
    }
}