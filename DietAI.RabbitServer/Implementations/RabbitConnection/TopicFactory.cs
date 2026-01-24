using System.Threading.Channels;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.RabbitConnection;

public class TopicFactory : ITopicFactory, IAsyncDisposable
{
    public IChannel ActiveChannel { get; private set; }

    public async Task<IChannel> CreateByConnectionAsync(IConnection connection)
    {
        ActiveChannel = await connection.CreateChannelAsync();
        return ActiveChannel;
    }

    public async ValueTask<QueueDeclareOk> DeclareQueueAsync(string queueName, IDictionary<string, object>? withArguments = null)
    {
        if (ActiveChannel is null)
        {
            throw new InvalidOperationException($"{nameof(ActiveChannel)} is null");
        }
        if (ActiveChannel.IsClosed)
        {
            throw new ChannelClosedException("Channel is closed");
        }
        
        return await ActiveChannel.QueueDeclareAsync(queueName, true, true, true, withArguments!);
    }

    public async ValueTask DisposeAsync()
    {
        await ActiveChannel.CloseAsync();
        await ActiveChannel.DisposeAsync();
    }
}