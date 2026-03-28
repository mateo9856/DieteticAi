using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;

namespace DieteticAI.UI.Tools;

public class TopicManager
{
    private readonly IRabbitConnectionFactory _connectionFactory;
    private ITopicFactory _topicFactory;
    private readonly IConfiguration _configuration;

    public TopicManager(IRabbitConnectionFactory connectionFactory, ITopicFactory topicFactory, IConfiguration configuration)
    {
        _connectionFactory = connectionFactory;
        _topicFactory = topicFactory;
        _configuration = configuration;
    }
    
    public async Task<IChannel> GetOrPrepareChannel()
    {
        if(_topicFactory is null)
            throw new NullReferenceException("Topic Factory is not initialized");

        if (_topicFactory.ActiveChannel is null || _topicFactory.ActiveChannel.IsClosed)
        {
            var hostName = _configuration["rabbitMq:hostName"] ?? "localhost";
            var userName = _configuration["rabbitMq:userName"] ?? "guest";
            var password = _configuration["rabbitMq:password"] ?? "guest";
            var portNumber = _configuration["rabbitMq:port"] ?? "5672";
            var certPath = _configuration["rabbitMq:certPath"] ?? "";
            
            var connection = await _connectionFactory
                .InitConnectionFactory()
                .WithUserName(userName)
                .WithPassword(password)
                .WithHostName(hostName)
                .WithPortNumber(int.Parse(portNumber))
                .WithTls(certPath)
                .WithVirtualHost("/")
                .PrepareConnectionAsync();

            return await _topicFactory.CreateByConnectionAsync(connection);
        }
        return _topicFactory.ActiveChannel;
    }
}