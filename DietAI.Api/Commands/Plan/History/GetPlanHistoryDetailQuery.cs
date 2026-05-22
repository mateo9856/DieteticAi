using DietAI.Api.Services.AiPlanSender.Models;
using MediatR;

namespace DietAI.Api.Commands.Plan.History;

public class GetPlanHistoryDetailQuery : IRequest<Diets?>
{
    public required string UserId { get; init; }
    public required int Id { get; init; }
}
