using System.Text;
using DietAI.AiKernel.Models.DTOs;
using DietAI.AiKernel.Services;
using DietAI.RabbitServer.Abstractions;
using DieteticAi.Models;
using DieteticAi.Tools;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace DietAI.AiKernel;

public class DietConcurrentRunner(
    ILogger<DietConcurrentRunner> logger,
    IReceiveService receiveService,
    ISenderService senderService,
    DietService dietService,
    TopicManager topicManager) : IHostedService
{
    private const string CreatePlanQueueName = "create_plan_request";
    private const string UpdatePlanQueueName = "update_plan_request";
    private const string DietPlanResponseQueueName = "diet_plan_response";

    private string? _createConsumerTag;
    private string? _updateConsumerTag;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting DietConcurrentRunner");
        await topicManager.GetOrPrepareChannel();

        _createConsumerTag = await receiveService.StartConsumingAsync(CreatePlanQueueName, OnCreateNewPlanReceived);
        _updateConsumerTag = await receiveService.StartConsumingAsync(UpdatePlanQueueName, OnUpdateNewPlanReceived);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping DietConcurrentRunner");
        if (_createConsumerTag != null)
            await receiveService.StopConsumingAsync(_createConsumerTag);
        if (_updateConsumerTag != null)
            await receiveService.StopConsumingAsync(_updateConsumerTag);
    }
    
    private async Task OnCreateNewPlanReceived(object sender, BasicDeliverEventArgs ev)
    {
        await ProcessMessageAsync<HumanDataDto>(CreatePlanQueueName, ev, "Creating new plan");
    }

    private async Task OnUpdateNewPlanReceived(object sender, BasicDeliverEventArgs ev)
    {
        await ProcessMessageAsync<UpdateHumanDataDto>(UpdatePlanQueueName, ev, "Updating plan");
    }

    private async Task ProcessMessageAsync<TRequest>(
        string queueName,
        BasicDeliverEventArgs ev,
        string operationName)
        where TRequest : HumanDataDto
    {
        try
        {
            logger.LogInformation("{OperationName}", operationName);

            var data = DeserializeConsumerData<PlanTopicRequest<TRequest>>(ev.Body.ToArray());
            var result = data.Request switch
            {
                UpdateHumanDataDto updateRequest => await dietService.UpdateExistingPlan(updateRequest),
                _ => await dietService.GenerateNewOrGetPlan(data.Request)
            };

            await senderService.SendToQueueAsync($"{DietPlanResponseQueueName}:{data.RequestId}", result, false);
            await receiveService.AckMessageAsync(queueName, ev);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid payload received from queue {QueueName}", queueName);
            await receiveService.RejectAsync(ev.DeliveryTag, requeue: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message from queue {QueueName}", queueName);
            await receiveService.RejectAsync(ev.DeliveryTag, requeue: true);
        }
    }

    private T DeserializeConsumerData<T>(byte[] data)
    {
        var request = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(request)
            ?? throw new ArgumentNullException($"Request has incorrect data. Contact with administrator.");
    } 
}
