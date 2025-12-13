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
        // Ensure queue exists before consuming (as requested)
        await _topicFactory.DeclareQueueAsync(queueName);

        var channel = _topicFactory.ActiveChannel 
            ?? throw new InvalidOperationException("No active channel available from ITopicFactory.");

        var eventingHandler = new AsyncEventingBasicConsumer(channel);
        eventingHandler.ReceivedAsync += async (model, ea) => await handler(ea);

        // BasicConsumeAsync is expected to return the consumerTag
        var consumerTag = await channel.BasicConsumeAsync(queueName, autoAck: autoAck, consumer: eventingHandler);

        // keep track so StopAll/StopConsuming can operate
        _consumers.TryAdd(consumerTag, queueName);

        return consumerTag;
    }

    public async Task RejectAsync(ulong deliveryTag, bool requeue = false)
    {
        var channel = _topicFactory.ActiveChannel ?? throw new InvalidOperationException("No active channel available from ITopicFactory.");
        await channel.BasicRejectAsync(deliveryTag, requeue);
    }

    public async Task RejectMultipleAsync(ulong deliveryTag, bool requeue = false)
    {
        var channel = _topicFactory.ActiveChannel ?? throw new InvalidOperationException("No active channel available from ITopicFactory.");
        // use BasicNack with multiple = true to reject multiple
        await channel.BasicNackAsync(deliveryTag, multiple: true, requeue: requeue);
    }

    public async Task StopConsumingAsync(string consumerTag)
    {
        var channel = _topicFactory.ActiveChannel ?? throw new InvalidOperationException("No active channel available from ITopicFactory.");
        await channel.BasicCancelAsync(consumerTag);

        _consumers.TryRemove(consumerTag, out _);
    }

    public async Task StopAllConsumersAsync()
    {
        var channel = _topicFactory.ActiveChannel ?? throw new InvalidOperationException("No active channel available from ITopicFactory.");

        // snapshot keys to avoid enumeration issues
        var tags = _consumers.Keys.ToArray();
        foreach (var tag in tags)
        {
            await channel.BasicCancelAsync(tag);
            _consumers.TryRemove(tag, out _);
        }
    }

    public async Task<BasicGetResult?> GetMessageAsync(string queueName, bool autoAck = true)
    {
        var channel = _topicFactory.ActiveChannel ?? throw new InvalidOperationException("No active channel available from ITopicFactory.");
        return await channel.BasicGetAsync(queueName, autoAck);
    }
}