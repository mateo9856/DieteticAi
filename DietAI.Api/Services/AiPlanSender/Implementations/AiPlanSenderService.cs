using System.Text.Json;
using DietAI.Api.Services.AiPlanSender.Abstractions;
using DietAI.Api.Services.AiPlanSender.Models;
using DietAI.Api.Services.AiPlanSender.Requests;
using DietAI.Api.Tools;
using DietAI.RabbitServer.Abstractions;
using RabbitMQ.Client.Exceptions;

namespace DietAI.Api.Services.AiPlanSender.Implementations;

public class AiPlanSenderService : IAiPlanSender
{
    private const string CreateDietPlanQueueName = "create_plan_request";
    private const string UpdatePlanQueueName = "update_plan_request";
    private const string DietPlanResponseQueueName = "diet_plan_response";

    private readonly ISenderService _senderService;
    private readonly IReceiveService _receiveService;
    private readonly TopicManager _topicManager;

    public AiPlanSenderService(
        ISenderService senderService,
        IReceiveService receiveService,
        TopicManager topicManager)
    {
        _senderService = senderService;
        _receiveService = receiveService;
        _topicManager = topicManager;
    }

    public Task<Diets> SendPlanRequestAsync(
        string userId,
        SendPlanRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync(CreateDietPlanQueueName, userId, request, cancellationToken);

    public Task<Diets> SendPlanUpdateAsync(
        string userId,
        SendUpdatePlanRequest update,
        CancellationToken cancellationToken = default) =>
        SendAsync(UpdatePlanQueueName, userId, update, cancellationToken);

    private async Task<Diets> SendAsync<TRequest>(
        string queueName,
        string userId,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : SendPlanRequest
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("User session must be loaded before requesting a diet plan");
        }

        try
        {
            await _topicManager.GetOrPrepareChannel();

            var requestWithContext = new PlanRequestWithContext<TRequest>
            {
                SendPlanRequest = request,
                UserId = userId,
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            await _senderService.SendToQueueAsync(queueName, requestWithContext, persistent: true);

            return await WaitForResponseAsync(
                requestWithContext.RequestId,
                TimeSpan.FromSeconds(30),
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Diet plan request timed out. The AI kernel did not respond in time.");
        }
        catch (Exception ex) when (ex is not TimeoutException)
        {
            throw new InvalidOperationException("Failed to send diet plan request", ex);
        }
    }

    private async Task<Diets> WaitForResponseAsync(
        string requestId,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var message = await _receiveService.GetMessageAsync(
                    $"{DietPlanResponseQueueName}:{requestId}",
                    autoAck: true);

                if (message is not null)
                {
                    var body = message.Body.ToArray();
                    var json = System.Text.Encoding.UTF8.GetString(body);
                    var diet = JsonSerializer.Deserialize<Diets>(json);

                    return diet ?? throw new InvalidOperationException("Failed to deserialize diet response");
                }
            }
            catch (OperationInterruptedException)
            {
            }

            await Task.Delay(500, cts.Token);
        }

        throw new TimeoutException($"No response received for request {requestId}");
    }
}

internal sealed class PlanRequestWithContext<T> where T : SendPlanRequest
{
    public required T SendPlanRequest { get; init; }
    public required string UserId { get; init; }
    public required string RequestId { get; init; }
    public DateTime Timestamp { get; init; }
}
