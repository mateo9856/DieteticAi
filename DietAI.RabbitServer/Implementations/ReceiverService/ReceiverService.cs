using DietAI.RabbitServer.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DietAI.RabbitServer.Implementations.ReceiverService;

public class ReceiverService : IReceiveService
{
    public async Task<string> StartConsumingAsync(string queueName, Func<BasicDeliverEventArgs, Task> handler, bool autoAck = false)
    {
        throw new NotImplementedException();
    }

    public async Task RejectAsync(ulong deliveryTag, bool requeue = false)
    {
        throw new NotImplementedException();
    }

    public async Task RejectMultipleAsync(ulong deliveryTag, bool requeue = false)
    {
        throw new NotImplementedException();
    }

    public async Task StopConsumingAsync(string consumerTag)
    {
        throw new NotImplementedException();
    }

    public async Task StopAllConsumersAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<BasicGetResult?> GetMessageAsync(string queueName, bool autoAck = true)
    {
        throw new NotImplementedException();
    }
}