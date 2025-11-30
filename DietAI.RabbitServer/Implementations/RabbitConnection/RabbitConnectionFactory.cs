using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.RabbitConnection;

public class RabbitConnectionFactory : IRabbitConnectionFactory
{
    public async Task<IConnection> PrepareConnectionAsync()
    {
        throw new NotImplementedException();
    }

    public ConnectionFactory WithUserName(string userName)
    {
        throw new NotImplementedException();
    }

    public ConnectionFactory WithPassword(string password)
    {
        throw new NotImplementedException();
    }

    public ConnectionFactory WithHostName(string hostName)
    {
        throw new NotImplementedException();
    }

    public ConnectionFactory WithVirtualHost(string virtualHost)
    {
        throw new NotImplementedException();
    }

    public ConnectionFactory WithTls()
    {
        throw new NotImplementedException();
    }
}