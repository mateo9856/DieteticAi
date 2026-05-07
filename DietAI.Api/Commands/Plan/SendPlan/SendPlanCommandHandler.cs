using DietAI.Api.Services.AiPlanSender.Abstractions;
using DietAI.Api.Services.AiPlanSender.Models;
using MediatR;

namespace DietAI.Api.Commands.Plan.SendPlan;

public class SendPlanCommandHandler : IRequestHandler<SendPlanCommand, Diets>
{
    private readonly IAiPlanSender _planSender;

    public SendPlanCommandHandler(IAiPlanSender planSender)
    {
        _planSender = planSender;
    }

    public async Task<Diets> Handle(SendPlanCommand request, CancellationToken cancellationToken)
    {
        return await _planSender.SendPlanRequestAsync(request.UserId, request.Request, cancellationToken);
    }
}