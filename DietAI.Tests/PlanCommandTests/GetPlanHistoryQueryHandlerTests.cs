using DietAI.Api.Commands.Plan.History;
using DietAI.Api.Data;
using DietAI.Api.Services.AiPlanSender.Enums;
using DietAI.Api.Services.AiPlanSender.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DietAI.Tests.PlanCommandTests;

public class GetPlanHistoryQueryHandlerTests
{
    [Test]
    public async Task Handle_ReturnsOnlyCurrentUserPlans_NewestFirst()
    {
        var dbContext = CreateDbContext();
        dbContext.Diets.AddRange(
            CreateDiet("user-1", DateTime.UtcNow.AddDays(-2), "old"),
            CreateDiet("user-2", DateTime.UtcNow, "other"),
            CreateDiet("user-1", DateTime.UtcNow.AddDays(-1), "new"));
        await dbContext.SaveChangesAsync();

        var handler = new GetPlanHistoryQueryHandler(dbContext);

        var result = await handler.Handle(new GetPlanHistoryQuery { UserId = "user-1" }, CancellationToken.None);

        result.Select(diet => diet.DietName).Should().Equal("new", "old");
    }

    [Test]
    public async Task HandleDetail_WhenPlanBelongsToAnotherUser_ReturnsNull()
    {
        var dbContext = CreateDbContext();
        var otherUserPlan = CreateDiet("user-2", DateTime.UtcNow, "other");
        dbContext.Diets.Add(otherUserPlan);
        await dbContext.SaveChangesAsync();

        var handler = new GetPlanHistoryDetailQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetPlanHistoryDetailQuery { UserId = "user-1", Id = otherUserPlan.Id },
            CancellationToken.None);

        result.Should().BeNull();
    }

    private static Diets CreateDiet(string userId, DateTime createdAtUtc, string dietName) => new()
    {
        DietName = dietName,
        Description = "Description",
        Age = 30,
        ForWeight = 80m,
        ForHeight = 180m,
        CaloricValue = 2400m,
        ForSex = SexEnum.Male,
        DietType = DietType.Standard,
        UserId = userId,
        GoalType = GoalType.MaintainWeight,
        ActivityLevel = ActivityLevel.Moderate,
        MealsPerDay = 3,
        CreatedAtUtc = createdAtUtc
    };

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
