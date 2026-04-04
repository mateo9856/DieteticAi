using DietAI.Api.Services.AiPlanSender.Models;
using DietAI.Api.Services.AiPlanSender.Requests;

namespace DietAI.Api.Services.AiPlanSender.Abstractions;

public interface IAiPlanSender
{
    Task<Diets> SendPlanRequestAsync(string userId, SendPlanRequest request, CancellationToken cancellationToken = default);
    Task<Diets> SendPlanUpdateAsync(string userId, SendUpdatePlanRequest update, CancellationToken cancellationToken = default);
}
