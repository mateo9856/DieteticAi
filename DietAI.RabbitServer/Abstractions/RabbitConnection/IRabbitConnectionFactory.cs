using RabbitMQ.Client;

namespace DietAI.RabbitServer.Abstractions.RabbitConnection;

public interface IRabbitConnectionFactory
{
    IConnection ActiveConnection { get; }
    Task<IConnection> PrepareConnectionAsync();
    IRabbitConnectionFactory WithUserName(string userName);
    IRabbitConnectionFactory WithPassword(string password);
    IRabbitConnectionFactory WithHostName(string hostName);
    IRabbitConnectionFactory WithVirtualHost(string virtualHost);
    IRabbitConnectionFactory WithTls();
}