using RabbitMQ.Client;

namespace DietAI.RabbitServer.Abstractions.RabbitConnection;

public interface IRabbitConnectionFactory
{
    Task<IConnection> PrepareConnectionAsync();
    ConnectionFactory WithUserName(string userName);
    ConnectionFactory WithPassword(string password);
    ConnectionFactory WithHostName(string hostName);
    ConnectionFactory WithVirtualHost(string virtualHost);
    ConnectionFactory WithTls();
}