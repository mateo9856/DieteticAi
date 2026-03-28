using RabbitMQ.Client;

namespace DietAI.RabbitServer.Abstractions.RabbitConnection;

public interface IRabbitConnectionFactory
{
    IConnection ActiveConnection { get; }
    ConnectionFactory GetConnectionFactory();
    Task<IConnection> PrepareConnectionAsync();
    IRabbitConnectionFactory InitConnectionFactory();
    IRabbitConnectionFactory WithUserName(string userName);
    IRabbitConnectionFactory WithPassword(string password);
    IRabbitConnectionFactory WithHostName(string hostName);
    IRabbitConnectionFactory WithPortNumber(int portNumber);
    IRabbitConnectionFactory WithVirtualHost(string virtualHost);
    IRabbitConnectionFactory WithTls(string certPath);
}