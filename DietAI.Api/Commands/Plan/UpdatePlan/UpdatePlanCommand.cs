using DietAI.Api.Services.AiPlanSender.Models;
using DietAI.Api.Services.AiPlanSender.Requests;
using MediatR;

namespace DietAI.Api.Commands.Plan.UpdatePlan;

public class UpdatePlanCommand : IRequest<Diets>
{
    public required string UserId { get; init; }
    public required SendUpdatePlanRequest Request { get; init; }
}