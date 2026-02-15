using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;

namespace DieteticAI.UI.Tools;

public class TopicManager
{
    private readonly IRabbitConnectionFactory _connectionFactory;
    private ITopicFactory _topicFactory;

    public TopicManager(IRabbitConnectionFactory connectionFactory, ITopicFactory topicFactory)
    {
        _connectionFactory = connectionFactory;
        _topicFactory = topicFactory;
    }
    public async Task<IChannel> GetOrPrepareChannel()
    {
        if(_topicFactory is null)
            throw new NullReferenceException("Topic Factory is not initialized");

        if (_topicFactory.ActiveChannel is null || _topicFactory.ActiveChannel.IsClosed)
        {
            var connection = await _connectionFactory
            .WithUserName("guest")
            .WithPassword("guest")
            .WithHostName("localhost")
            .PrepareConnectionAsync();

            return await _topicFactory.CreateByConnectionAsync(connection);
        }
        return _topicFactory.ActiveChannel;
    }
}