using DietAI.Api.Data;
using DietAI.Api.Services.AiPlanSender.Abstractions;
using DietAI.Api.Services.AiPlanSender.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DietAI.Api.Commands.Plan.UpdatePlan;

public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, Diets>
{
    private readonly IAiPlanSender _planSender;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UpdatePlanCommandHandler> _logger;

    public UpdatePlanCommandHandler(
        IAiPlanSender planSender,
        ApplicationDbContext dbContext,
        ILogger<UpdatePlanCommandHandler> logger)
    {
        _planSender = planSender;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Diets> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var diet = await _planSender.SendPlanUpdateAsync(request.UserId, request.Request, cancellationToken);
        ApplyPersistenceSnapshot(diet, request);

        try
        {
            await _dbContext.Diets.AddAsync(diet, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save updated diet plan for user {UserId}", request.UserId);
            throw new InvalidOperationException("Failed to save updated diet plan", ex);
        }

        return diet;
    }

    private static void ApplyPersistenceSnapshot(Diets diet, UpdatePlanCommand command)
    {
        diet.Id = 0;
        diet.UserId = command.UserId;
        diet.GoalType = command.Request.GoalType;
        diet.ActivityLevel = command.Request.ActivityLevel;
        diet.MealsPerDay = command.Request.MealsPerDay;
        diet.Allergies = command.Request.Allergies;
        diet.ExcludedIngredients = command.Request.ExcludedIngredients;
        diet.CreatedAtUtc = DateTime.UtcNow;
    }
}
