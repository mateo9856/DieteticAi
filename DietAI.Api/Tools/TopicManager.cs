using DietAI.Api.Options;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DietAI.Api.Tools;

public class TopicManager
{
    private readonly IRabbitConnectionFactory _connectionFactory;
    private readonly ITopicFactory _topicFactory;
    private readonly RabbitMqOptions _rabbitMqOptions;

    public TopicManager(
        IRabbitConnectionFactory connectionFactory,
        ITopicFactory topicFactory,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _connectionFactory = connectionFactory;
        _topicFactory = topicFactory;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    public async Task<IChannel> GetOrPrepareChannel()
    {
        if (_topicFactory.ActiveChannel is null || _topicFactory.ActiveChannel.IsClosed)
        {
            var connection = await _connectionFactory
                .InitConnectionFactory()
                .WithUserName(_rabbitMqOptions.UserName)
                .WithPassword(_rabbitMqOptions.Password)
                .WithHostName(_rabbitMqOptions.HostName)
                .WithVirtualHost(_rabbitMqOptions.VirtualHost)
                .PrepareConnectionAsync();

            return await _topicFactory.CreateByConnectionAsync(connection);
        }

        return _topicFactory.ActiveChannel;
    }
}
