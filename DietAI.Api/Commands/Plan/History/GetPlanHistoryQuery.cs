using DietAI.Api.Services.AiPlanSender.Models;
using MediatR;

namespace DietAI.Api.Commands.Plan.History;

public class GetPlanHistoryQuery : IRequest<IReadOnlyList<Diets>>
{
    public required string UserId { get; init; }
}
