using DietAI.Api.Commands.Plan.SendPlan;
using DietAI.Api.Data;
using DietAI.Api.Services.AiPlanSender.Abstractions;
using DietAI.Api.Services.AiPlanSender.Enums;
using DietAI.Api.Services.AiPlanSender.Models;
using DietAI.Api.Services.AiPlanSender.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DietAI.Tests.PlanCommandTests;

public class SendPlanCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenPlanGenerated_SavesWithJwtUserAndPreferenceSnapshot()
    {
        var dbContext = CreateDbContext();
        var planSender = Substitute.For<IAiPlanSender>();
        planSender.SendPlanRequestAsync("jwt-user", Arg.Any<SendPlanRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Diets
            {
                Id = 99,
                DietName = "Generated",
                Description = "Generated plan",
                Age = 30,
                ForWeight = 80m,
                ForHeight = 180m,
                CaloricValue = 2400m,
                ForSex = SexEnum.Male,
                DietType = DietType.Standard,
                UserId = "untrusted"
            });

        var request = new SendPlanRequest
        {
            Age = 30,
            ActualWeight = 80m,
            ActualHeight = 180m,
            Sex = SexEnum.Male,
            DietType = DietType.Standard,
            CaloricDemand = 2400m,
            GoalType = GoalType.LoseWeight,
            ActivityLevel = ActivityLevel.Active,
            MealsPerDay = 5,
            Allergies = ["peanuts"],
            ExcludedIngredients = ["mushrooms"]
        };

        var handler = new SendPlanCommandHandler(
            planSender,
            dbContext,
            NullLogger<SendPlanCommandHandler>.Instance);

        var result = await handler.Handle(
            new SendPlanCommand { UserId = "jwt-user", Request = request },
            CancellationToken.None);

        var saved = await dbContext.Diets.SingleAsync();
        saved.UserId.Should().Be("jwt-user");
        saved.GoalType.Should().Be(GoalType.LoseWeight);
        saved.ActivityLevel.Should().Be(ActivityLevel.Active);
        saved.MealsPerDay.Should().Be(5);
        saved.Allergies.Should().BeEquivalentTo("peanuts");
        saved.ExcludedIngredients.Should().BeEquivalentTo("mushrooms");
        saved.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Id.Should().Be(saved.Id);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
