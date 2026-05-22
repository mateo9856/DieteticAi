using DietAI.Api.Data;
using DietAI.Api.Services.AiPlanSender.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DietAI.Api.Commands.Plan.History;

public class GetPlanHistoryDetailQueryHandler : IRequestHandler<GetPlanHistoryDetailQuery, Diets?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetPlanHistoryDetailQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Diets?> Handle(GetPlanHistoryDetailQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Diets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                diet => diet.Id == request.Id && diet.UserId == request.UserId,
                cancellationToken);
    }
}
