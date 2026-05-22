using DietAI.Api.Data;
using DietAI.Api.Services.AiPlanSender.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DietAI.Api.Commands.Plan.History;

public class GetPlanHistoryQueryHandler : IRequestHandler<GetPlanHistoryQuery, IReadOnlyList<Diets>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetPlanHistoryQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Diets>> Handle(GetPlanHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Diets
            .AsNoTracking()
            .Where(diet => diet.UserId == request.UserId)
            .OrderByDescending(diet => diet.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
