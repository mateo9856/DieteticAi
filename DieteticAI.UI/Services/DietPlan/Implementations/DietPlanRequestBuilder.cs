using DieteticAI.UI.Services.AiPlanSender.Requests;
using DieteticAI.UI.Services.DietPlan.Abstractions;
using DieteticAI.UI.Tools;

namespace DieteticAI.UI.Services.DietPlan.Implementations;

public sealed class DietPlanRequestBuilder(IPreferenceTextParser preferenceTextParser) : IDietPlanRequestBuilder
{
    private readonly DietsExtensions _dietExtensions = new();

    public SendPlanRequest Build(SendPlanRequest formRequest, string allergiesText, string excludedIngredientsText)
    {
        return new SendPlanRequest
        {
            Age = formRequest.Age,
            ActualWeight = formRequest.ActualWeight,
            ActualHeight = formRequest.ActualHeight,
            Sex = formRequest.Sex,
            DietType = formRequest.DietType,
            GoalType = formRequest.GoalType,
            ActivityLevel = formRequest.ActivityLevel,
            MealsPerDay = formRequest.MealsPerDay,
            CaloricDemand = _dietExtensions.CalculateCalories(formRequest.Age, formRequest.ActualWeight, formRequest.Sex),
            Allergies = preferenceTextParser.Parse(allergiesText, 10, "Allergies"),
            ExcludedIngredients = preferenceTextParser.Parse(excludedIngredientsText, 20, "Excluded ingredients")
        };
    }
}
