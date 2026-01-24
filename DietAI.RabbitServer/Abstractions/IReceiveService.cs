using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DietAI.RabbitServer.Abstractions;

public interface IReceiveService
{
    Task<string> StartConsumingAsync(string queueName, Func<BasicDeliverEventArgs, Task> handler, bool autoAck = false);

    Task RejectAsync(ulong deliveryTag, bool requeue = false);

    Task RejectMultipleAsync(ulong deliveryTag, bool requeue = false);

    Task StopConsumingAsync(string consumerTag);

    Task StopAllConsumersAsync();

    Task<BasicGetResult?> GetMessageAsync(string queueName, bool autoAck = true);
}