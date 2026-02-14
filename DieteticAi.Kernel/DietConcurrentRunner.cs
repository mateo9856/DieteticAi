using System.Text;
using DietAI.AiKernel.Models.DTOs;
using DietAI.AiKernel.Services;
using DietAI.RabbitServer.Abstractions;
using DieteticAi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace DietAI.AiKernel;

public class DietConcurrentRunner(
    ILogger<DietConcurrentRunner> logger,
    IReceiveService receiveService,
    ISenderService senderService,
    DietService dietService)
{
    private const string CreatePlanQueueName = "create_plan_request";
    private const string UpdatePlanQueueName = "update_plan_request";
    private const string DietPlanResponseQueueName = "diet_plan_response";

    public async Task Run(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting DietConcurrentRunner");
        await receiveService.StartConsumingAsync(CreatePlanQueueName, OnCreateNewPlanReceived);
        await receiveService.StartConsumingAsync(UpdatePlanQueueName, OnUpdateNewPlanReceived);
    }
    
    private async Task OnCreateNewPlanReceived(object sender, BasicDeliverEventArgs ev)
    {
        logger.LogInformation("Creating new plan");
        var data = DeserializeConsumerData<PlanTopicRequest<HumanDataDto>>(ev.Body.ToArray());
        await receiveService.AckMessageAsync(CreatePlanQueueName, ev);

        var result = dietService.GenerateNewOrGetPlan(data.Request);

        await senderService.SendToQueueAsync($"{DietPlanResponseQueueName}:{data.RequestId}", result, false);
    }

    private async Task OnUpdateNewPlanReceived(object sender, BasicDeliverEventArgs ev)
    {
        logger.LogInformation("Updating new plan");
        var data = DeserializeConsumerData<PlanTopicRequest<UpdateHumanDataDto>>(ev.Body.ToArray());
        await receiveService.AckMessageAsync(UpdatePlanQueueName, ev);
        
        var result = dietService.GenerateNewOrGetPlan(data.Request);

        await senderService.SendToQueueAsync($"{DietPlanResponseQueueName}:{data.RequestId}", result, false);
    }

    private T DeserializeConsumerData<T>(byte[] data)
    {
        var request = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(request)
            ?? throw new ArgumentNullException($"Request has incorrect data. Contact with administrator.");
    } 
}