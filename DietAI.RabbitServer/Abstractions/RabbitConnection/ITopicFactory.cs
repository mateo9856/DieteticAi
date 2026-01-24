using RabbitMQ.Client;

namespace DietAI.RabbitServer.Abstractions.RabbitConnection;

public interface ITopicFactory
{
    IChannel ActiveChannel { get; }
    Task<IChannel> CreateByConnectionAsync(IConnection connection);
    ValueTask<QueueDeclareOk> DeclareQueueAsync(string queueName, IDictionary<string, object>? withArguments = null);
}