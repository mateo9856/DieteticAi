using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.RabbitConnection;

public class RabbitConnectionFactory : IRabbitConnectionFactory, IAsyncDisposable
{
    private readonly ConnectionFactory _connectionFactory;

    private const string HostNameRequiredMessage = "RabbitMQ Factory: HostName is required";

    public IConnection ActiveConnection { get; private set; }
    
    public RabbitConnectionFactory()
    {
        _connectionFactory = new ConnectionFactory();
    }
    
    public async Task<IConnection> PrepareConnectionAsync()
    {
        ActiveConnection = await _connectionFactory.CreateConnectionAsync();
        return ActiveConnection;
    }

    public IRabbitConnectionFactory WithUserName(string userName)
    {
        _connectionFactory.UserName = userName;
        return this;
    }

    public IRabbitConnectionFactory WithPassword(string password)
    {
        _connectionFactory.Password = password;
        return this;
    }

    public IRabbitConnectionFactory WithHostName(string hostName)
    {
        _connectionFactory.HostName = hostName;
        return this;
    }

    public IRabbitConnectionFactory WithVirtualHost(string virtualHost)
    {
        _connectionFactory.VirtualHost = virtualHost;
        return this;
    }

    public IRabbitConnectionFactory WithTls()
    {
        if (_connectionFactory?.HostName is null)
            throw new InvalidOperationException(HostNameRequiredMessage);

        _connectionFactory.Ssl = new SslOption
        {
            Enabled = true,
            ServerName = _connectionFactory.HostName
        };
        return this;
    }

    public async ValueTask DisposeAsync()
    {
        await ActiveConnection.CloseAsync();
        await ActiveConnection.DisposeAsync();
    }
}