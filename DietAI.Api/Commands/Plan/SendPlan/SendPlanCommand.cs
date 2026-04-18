using DietAI.Api.Services.AiPlanSender.Models;
using DietAI.Api.Services.AiPlanSender.Requests;
using MediatR;

namespace DietAI.Api.Commands.Plan.SendPlan;

public class SendPlanCommand : IRequest<Diets>
{
    public required string UserId { get; init; }
    public required SendPlanRequest Request { get; init; }
}