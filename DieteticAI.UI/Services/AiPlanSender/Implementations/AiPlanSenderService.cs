using System.Text.Json;
using DietAI.RabbitServer.Abstractions;
using DieteticAI.UI.Services.AiPlanSender.Abstractions;
using DieteticAI.UI.Services.AiPlanSender.Models;
using DieteticAI.UI.Services.AiPlanSender.Requests;
using DieteticAI.UI.Tools;

namespace DieteticAI.UI.Services.AiPlanSender.Implementations;

public class AiPlanSenderService : IAiPlanSender
{
    private const string CreateDietPlanQueueName = "create_plan_request";
    private const string UpdatePlanQueueName = "update_plan_request";
    private const string DietPlanResponseQueueName = "diet_plan_response";
    
    private readonly ISenderService _senderService;
    private readonly IReceiveService _receiveService;
    private readonly SessionManager _sessionManager;
    private readonly TopicManager _topicManager;
    
    public AiPlanSenderService(
        ISenderService senderService,
        IReceiveService receiveService,
        SessionManager sessionManager,
        TopicManager topicManager)
    {
        _senderService = senderService;
        _receiveService = receiveService;
        _sessionManager = sessionManager;
        _topicManager = topicManager;
    }

    /// <summary>
    /// Sends a diet plan request via RabbitMQ and waits for the AI kernel response
    /// </summary>
    public async Task<Diets> SendPlanRequestAsync(SendPlanRequest request, CancellationToken cancellationToken = default)
    {
        if (!_sessionManager.IsUserLoaded)
            throw new InvalidOperationException("User session must be loaded before requesting a diet plan");

        try
        {
            await PrepareChannel();
            
            // Add user context to request
            var requestWithContext = new PlanRequestWithContext<SendPlanRequest>
            {
                SendPlanRequest = request,
                UserId = _sessionManager.UserId!,
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Send request to AI kernel
            await _senderService.SendToQueueAsync(
                CreateDietPlanQueueName,
                requestWithContext,
                persistent: true);

            // Wait for response (with timeout)
            var responseTimeout = TimeSpan.FromSeconds(30);
            var responseTask = WaitForResponseAsync(requestWithContext.RequestId, responseTimeout, cancellationToken);
            
            var diet = await responseTask;
            return diet;
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Diet plan request timed out. The AI kernel did not respond in time.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send diet plan request", ex);
        }
    }

    public async Task<Diets> SendPlanUpdateAsync(SendUpdatePlanRequest update, CancellationToken cancellationToken = default)
    {
    
        if (!_sessionManager.IsUserLoaded)
            throw new InvalidOperationException("User session must be loaded before requesting a diet plan");

        try
        {
            await PrepareChannel();
            
            // Add user context to request
            var requestWithContext = new PlanRequestWithContext<SendUpdatePlanRequest>
            {
                SendPlanRequest = update,
                UserId = _sessionManager.UserId!,
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Send request to AI kernel
            await _senderService.SendToQueueAsync(
                UpdatePlanQueueName,
                requestWithContext,
                persistent: true);

            // Wait for response (with timeout)
            var responseTimeout = TimeSpan.FromSeconds(30);
            var responseTask = WaitForResponseAsync(requestWithContext.RequestId, responseTimeout, cancellationToken);
            
            var diet = await responseTask;
            return diet;
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Diet plan request timed out. The AI kernel did not respond in time.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send diet plan request", ex);
        }
    }

    /// <summary>
    /// Listens for the response from the AI kernel
    /// </summary>
    private async Task<Diets> WaitForResponseAsync(string requestId, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        // Poll for response (you might want to implement a more robust solution with SignalR or similar)
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
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

            // Wait before polling again
            await Task.Delay(500, cts.Token);
        }

        throw new TimeoutException($"No response received for request {requestId}");
    }

    private async Task PrepareChannel() => await _topicManager.GetOrPrepareChannel();

}

/// <summary>
/// Extended request with metadata for tracking and routing
/// </summary>
internal class PlanRequestWithContext<T> where T : SendPlanRequest
{
    public required T SendPlanRequest { get; init; }
    public required string UserId { get; init; }
    public required string RequestId { get; init; }
    public DateTime Timestamp { get; init; }
}
