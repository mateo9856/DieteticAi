using DietAI.Api.Commands.Plan.SendPlan;
using DietAI.Api.Services.AiPlanSender.Enums;
using DietAI.Api.Services.AiPlanSender.Requests;
using FluentAssertions;

namespace DietAI.Tests.PlanCommandTests;

public class SendPlanCommandValidatorTests
{
    [Test]
    public void Validate_WhenMealPreferencesAreValid_ReturnsNoPreferenceErrors()
    {
        var validator = new SendPlanCommandValidator();
        var command = new SendPlanCommand
        {
            UserId = "user-1",
            Request = ValidRequest()
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WhenMealPreferencesAreInvalid_ReturnsPreferenceErrors()
    {
        var validator = new SendPlanCommandValidator();
        var request = ValidRequest();
        request.MealsPerDay = 7;
        request.Allergies = Enumerable.Range(1, 11).Select(index => $"allergy-{index}").ToList();
        request.ExcludedIngredients = Enumerable.Range(1, 21).Select(index => $"ingredient-{index}").ToList();

        var command = new SendPlanCommand
        {
            UserId = "user-1",
            Request = request
        };

        var result = validator.Validate(command);

        result.Errors.Select(error => error.PropertyName).Should().Contain([
            nameof(SendPlanRequest.MealsPerDay),
            nameof(SendPlanRequest.Allergies),
            nameof(SendPlanRequest.ExcludedIngredients)
        ]);
    }

    private static SendPlanRequest ValidRequest() => new()
    {
        Age = 30,
        ActualWeight = 80m,
        ActualHeight = 180m,
        Sex = SexEnum.Male,
        DietType = DietType.Standard,
        CaloricDemand = 2400m,
        GoalType = GoalType.MaintainWeight,
        ActivityLevel = ActivityLevel.Moderate,
        MealsPerDay = 3
    };
}
