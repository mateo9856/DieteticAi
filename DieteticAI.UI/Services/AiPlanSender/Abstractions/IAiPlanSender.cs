using DieteticAI.UI.Services.AiPlanSender.Models;

namespace DieteticAI.UI.Services.AiPlanSender.Abstractions;

public interface IAiPlanSender
{
    Task<Diets> SendPlanRequestAsync(SendPlanRequest request, CancellationToken cancellationToken = default);
    Task<Diets> SendPlanUpdateAsync(SendPlanRequest update, CancellationToken cancellationToken = default);
}