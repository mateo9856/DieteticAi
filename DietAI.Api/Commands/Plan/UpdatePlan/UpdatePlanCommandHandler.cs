using DietAI.Api.Services.AiPlanSender.Abstractions;
using DietAI.Api.Services.AiPlanSender.Models;
using MediatR;

namespace DietAI.Api.Commands.Plan.UpdatePlan;

public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, Diets>
{
    private readonly IAiPlanSender _planSender;

    public UpdatePlanCommandHandler(IAiPlanSender planSender)
    {
        _planSender = planSender;
    }

    public async Task<Diets> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        return await _planSender.SendPlanUpdateAsync(request.UserId, request.Request, cancellationToken);
    }
}