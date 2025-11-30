using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.RabbitConnection;

public class TopicFactory : ITopicFactory
{
    public async Task<IChannel> CreateByConnectionAsync(IConnection connection)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<QueueDeclareOk> DeclareQueueAsync(string queueName, IDictionary<string, object>? withArguments = null)
    {
        throw new NotImplementedException();
    }
}