using DieteticAI.UI.Services.AiPlanSender.Models;
using DieteticAI.UI.Services.AiPlanSender.Requests;

namespace DieteticAI.UI.Services.AiPlanSender.Abstractions;

public interface IAiPlanSender
{
    Task<Diets> SendPlanRequestAsync(SendPlanRequest request, CancellationToken cancellationToken = default);
    Task<Diets> SendPlanUpdateAsync(SendUpdatePlanRequest update, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Diets>> GetPlanHistoryAsync(CancellationToken cancellationToken = default);
    Task<Diets> GetPlanHistoryDetailAsync(int id, CancellationToken cancellationToken = default);
}
