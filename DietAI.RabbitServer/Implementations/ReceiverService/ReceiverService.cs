using System.Collections.Concurrent;
using DietAI.RabbitServer.Abstractions;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DietAI.RabbitServer.Implementations.ReceiverService;

public class ReceiverService : IReceiveService
{
    private readonly ITopicFactory _topicFactory;
    private readonly ConcurrentDictionary<string, string> _consumers = new();

    public ReceiverService(ITopicFactory topicFactory)
    {
        _topicFactory = topicFactory;
    }

    public async Task<string> StartConsumingAsync(string queueName, Func<BasicDeliverEventArgs, Task> handler, bool autoAck = false)
    {
        await _topicFactory.DeclareQueueAsync(queueName);

        var channel = EnsureChannelIsActive();
        
        var eventingHandler = new AsyncEventingBasicConsumer(channel);
        eventingHandler.ReceivedAsync += async (model, ea) => await handler(ea);

        var consumerTag = await channel.BasicConsumeAsync(queueName, autoAck: autoAck, consumer: eventingHandler);

        _consumers.TryAdd(consumerTag, queueName);

        return consumerTag;
    }

    public async Task RejectAsync(ulong deliveryTag, bool requeue = false)
    {
        var channel = EnsureChannelIsActive();
        await channel.BasicRejectAsync(deliveryTag, requeue);
    }

    public async Task RejectMultipleAsync(ulong deliveryTag, bool requeue = false)
    {
        var channel = EnsureChannelIsActive();
        
        await channel.BasicNackAsync(deliveryTag, multiple: true, requeue: requeue);
    }

    public async Task StopConsumingAsync(string consumerTag)
    {
        var channel = EnsureChannelIsActive();
        await channel.BasicCancelAsync(consumerTag);

        _consumers.TryRemove(consumerTag, out _);
    }

    public async Task StopAllConsumersAsync()
    {
        var channel = EnsureChannelIsActive();
        
        var tags = _consumers.Keys.ToArray();
        foreach (var tag in tags)
        {
            await channel.BasicCancelAsync(tag);
            _consumers.TryRemove(tag, out _);
        }
    }

    public async Task<BasicGetResult?> GetMessageAsync(string queueName, bool autoAck = true)
    {
        var channel = EnsureChannelIsActive();
        return await channel.BasicGetAsync(queueName, autoAck);
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