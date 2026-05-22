using DietAI.Api.Services.AiPlanSender.Requests;
using FluentValidation;

namespace DietAI.Api.Commands.Plan;

internal static class MealPreferenceValidationExtensions
{
    public static IRuleBuilderOptionsConditions<T, SendPlanRequest> ValidMealPreferences<T>(
        this IRuleBuilder<T, SendPlanRequest> ruleBuilder)
    {
        return ruleBuilder.Custom((request, context) =>
        {
            if (!Enum.IsDefined(request.GoalType))
            {
                context.AddFailure(nameof(request.GoalType), "Goal type is invalid.");
            }

            if (!Enum.IsDefined(request.ActivityLevel))
            {
                context.AddFailure(nameof(request.ActivityLevel), "Activity level is invalid.");
            }

            if (request.MealsPerDay is < 2 or > 6)
            {
                context.AddFailure(nameof(request.MealsPerDay), "Meals per day must be between 2 and 6.");
            }

            ValidatePreferenceList(context, nameof(request.Allergies), request.Allergies, 10);
            ValidatePreferenceList(context, nameof(request.ExcludedIngredients), request.ExcludedIngredients, 20);
        });
    }

    private static void ValidatePreferenceList<T>(
        ValidationContext<T> context,
        string propertyName,
        IReadOnlyCollection<string>? values,
        int maxItems)
    {
        if (values is null)
        {
            return;
        }

        if (values.Count > maxItems)
        {
            context.AddFailure(propertyName, $"{propertyName} cannot contain more than {maxItems} items.");
        }

        if (values.Any(string.IsNullOrWhiteSpace))
        {
            context.AddFailure(propertyName, $"{propertyName} cannot contain empty items.");
        }
    }
}
