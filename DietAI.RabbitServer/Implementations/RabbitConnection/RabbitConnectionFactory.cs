using System.Net.Security;
using System.Security.Authentication;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using RabbitMQ.Client;

namespace DietAI.RabbitServer.Implementations.RabbitConnection;

public class RabbitConnectionFactory : IRabbitConnectionFactory, IAsyncDisposable
{
    private ConnectionFactory _connectionFactory;

    private const string HostNameRequiredMessage = "RabbitMQ Factory: HostName is required";

    public IConnection ActiveConnection { get; private set; }

    public ConnectionFactory GetConnectionFactory()
        => _connectionFactory;

    public async Task<IConnection> PrepareConnectionAsync()
    {
        ActiveConnection = await _connectionFactory.CreateConnectionAsync();
        return ActiveConnection;
    }

    public IRabbitConnectionFactory InitConnectionFactory()
    {
        _connectionFactory = new ConnectionFactory();
        return this;
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

    public IRabbitConnectionFactory WithPortNumber(int portNumber)
    {  
        _connectionFactory.Port = portNumber;
        return this;
    }

    public IRabbitConnectionFactory WithVirtualHost(string virtualHost)
    {
        _connectionFactory.VirtualHost = virtualHost;
        return this;
    }

    public IRabbitConnectionFactory WithTls(string certPath)
    {
        if (_connectionFactory?.HostName is null)
            throw new InvalidOperationException(HostNameRequiredMessage);

        _connectionFactory.Ssl = new SslOption
        {
            Enabled = true,
            ServerName = _connectionFactory.HostName,
            Version = SslProtocols.Tls12 | SslProtocols.Tls13,
            CertPath = certPath,
            AcceptablePolicyErrors = SslPolicyErrors.None,
        };
        return this;
    }

    public async ValueTask DisposeAsync()
    {
        await ActiveConnection.CloseAsync();
        await ActiveConnection.DisposeAsync();
    }
}