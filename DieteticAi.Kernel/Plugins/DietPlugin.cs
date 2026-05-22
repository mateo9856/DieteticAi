using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;
using DieteticAi.Constaints;
using DieteticAi.Models;
using DieteticAi.Tools.Wrappers;
using Microsoft.SemanticKernel;

namespace DieteticAi.Plugins;

public class DietPlugin
{
    private static readonly TimeSpan ModelExecutionTimeout = TimeSpan.FromSeconds(60);
    private readonly IList<Diets> _diets;
    private readonly IKernelWrapper _kernelWrapper;

    public DietPlugin(IList<Diets> diets, IKernelWrapper kernel)
    {
        _diets = diets;
        _kernelWrapper = kernel;
    }

    [KernelFunction("update_existing_plan")]
    [Description("Define previous plan and then update to new plan details.")]
    public async Task<Diets> UpdatePlanByPrompt(
        [Description("Actual age of person")] int actualAge,
        [Description("Previous age of person")] int previousAge,
        [Description("Current weight in kg")] decimal currentWeight,
        [Description("Previous weight in kg")] decimal previousWeight,
        [Description("Current height in cm")] decimal currentHeight,
        [Description("Previous height in kg")] decimal previousHeight,
        [Description("Current caloric demand")] decimal currentCaloricDemand,
        [Description("Previous caloric demand")] decimal previousCaloricDemand,
        [Description("Sex of the Person (Male/Female/Unbinary)")] SexEnum sex,
        [Description("Type of diet")] DietType dietType,
        [Description("Weight goal")] GoalType goalType = GoalType.MaintainWeight,
        [Description("Activity level")] ActivityLevel activityLevel = ActivityLevel.Sedentary,
        [Description("Meals per day")] int mealsPerDay = 3,
        [Description("Allergies to avoid")] IReadOnlyCollection<string>? allergies = null,
        [Description("Ingredients to exclude")] IReadOnlyCollection<string>? excludedIngredients = null
        )
    {
        var existingPlans = _diets.Where(diet => 
            diet.Age == previousAge 
            && diet.ForWeight == previousWeight
            && diet.ForHeight == previousHeight
            && diet.CaloricValue == previousCaloricDemand
            && diet.ForSex == sex
            && diet.DietType == dietType
            && diet.GoalType == goalType
            && diet.ActivityLevel == activityLevel
            && diet.MealsPerDay == mealsPerDay
            && PreferenceListsEqual(diet.Allergies, allergies)
            && PreferenceListsEqual(diet.ExcludedIngredients, excludedIngredients));

        var existingPlan = existingPlans.FirstOrDefault();
        if (existingPlan is null)
        {
            throw new Exception("Plan does not exist, please generate new plan.");
        }

        var updatedPlan = await UpdatePlanForPrompt(
            existingPlan.Id,
            actualAge,
            currentWeight,
            currentHeight,
            sex,
            dietType,
            currentCaloricDemand,
            previousWeight,
            previousHeight,
            previousCaloricDemand,
            goalType,
            activityLevel,
            mealsPerDay,
            allergies,
            excludedIngredients);

        var index = _diets.IndexOf(existingPlan);
        _diets[index] = updatedPlan;

        return updatedPlan;
    }
    
    [KernelFunction("get_diet_or_generate")]
    [Description("Find suggested plan if it exist, otherwise generate new plan.")]
    public async Task<Diets> GetPlanFromListOrPrompt(
        [Description("Age of the person")] int age,
        [Description("Current weight in kg")] decimal currentWeight,
        [Description("Current height in cm")] decimal currentHeight,
        [Description("Current caloric demand")] decimal currentCaloricDemand,
        [Description("Sex of the Person (Male/Female/Unbinary)")] SexEnum sex,
        [Description("Type of diet")] DietType dietType,
        [Description("Weight goal")] GoalType goalType = GoalType.MaintainWeight,
        [Description("Activity level")] ActivityLevel activityLevel = ActivityLevel.Sedentary,
        [Description("Meals per day")] int mealsPerDay = 3,
        [Description("Allergies to avoid")] IReadOnlyCollection<string>? allergies = null,
        [Description("Ingredients to exclude")] IReadOnlyCollection<string>? excludedIngredients = null
    )
    {
        var ageCorrect = (int a) => age >= 15 && (age - a <= 2);
        var weightCorrect = (decimal b) => (currentWeight - b <= 5 || b + 5 <= currentWeight);
        var heightCorrect = (decimal c) => (currentHeight - c <= 5 || c + 5 <= currentHeight);
        var caloricCorrect = (decimal d) => (currentCaloricDemand - d <= 50 || d + 50 <= currentCaloricDemand);
        
        var plansWithoutCaloric = _diets.Where(d => 
            ageCorrect(d.Age) 
            && weightCorrect(d.ForWeight)
            && heightCorrect(d.ForHeight)
            && d.ForSex == sex
            && d.DietType == dietType
            && d.GoalType == goalType
            && d.ActivityLevel == activityLevel
            && d.MealsPerDay == mealsPerDay
            && PreferenceListsEqual(d.Allergies, allergies)
            && PreferenceListsEqual(d.ExcludedIngredients, excludedIngredients));

        Diets? findingPlan = null;
        if (currentCaloricDemand == 0)
            findingPlan = plansWithoutCaloric.FirstOrDefault();
        else
            findingPlan = plansWithoutCaloric.FirstOrDefault(cal => caloricCorrect(cal.CaloricValue));
        
        if (findingPlan is not null)
        {
            return findingPlan;
        }

        var parametrizeCaloric = currentCaloricDemand == 0 ? (decimal?)null : currentCaloricDemand;
        var generatedDiet = await GenerateNewPlanForPrompt(
            age,
            currentWeight,
            currentHeight,
            sex,
            dietType,
            parametrizeCaloric,
            goalType,
            activityLevel,
            mealsPerDay,
            allergies,
            excludedIngredients);
        _diets.Add(generatedDiet);
        
        return generatedDiet;
    }

    protected virtual async Task<Diets> GenerateNewPlanForPrompt(
        int age,
        decimal currentWeight,
        decimal currentHeight,
        SexEnum sex,
        DietType dietType,
        decimal? caloric,
        GoalType goalType,
        ActivityLevel activityLevel,
        int mealsPerDay,
        IReadOnlyCollection<string>? allergies,
        IReadOnlyCollection<string>? excludedIngredients)
    {
        string promptBuilder = DietPrompts.CreatePlanPrompt(_diets.Count + 1) + PreferencePromptSection();
        var kernelArguments = new KernelArguments
        {
            ["age"] = age,
            ["weight"] = currentWeight,
            ["height"] = currentHeight,
            ["sex"] = sex.ToString(),
            ["dietType"] = dietType.ToString(),
            ["goalType"] = goalType.ToString(),
            ["activityLevel"] = activityLevel.ToString(),
            ["mealsPerDay"] = mealsPerDay.ToString(),
            ["allergies"] = FormatPreferenceList(allergies),
            ["excludedIngredients"] = FormatPreferenceList(excludedIngredients),
        };

        if (caloric is not null)
        {
            promptBuilder += @"
            - CaloricDemand:  {{$caloricDemand}} kcal
            ";
            kernelArguments["caloricDemand"] = caloric.ToString();
        }
        var returnedPlan = await InvokePromptWithTimeoutAsync(promptBuilder, kernelArguments);
        if (string.IsNullOrWhiteSpace(returnedPlan))
        {
            throw new Exception("Model returned empty response");
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(returnedPlan);
            var diet = ParseDiet(
                jsonDocument.RootElement,
                fallbackHeight: currentHeight,
                fallbackDietType: dietType);
            ApplyPreferenceSnapshot(diet, goalType, activityLevel, mealsPerDay, allergies, excludedIngredients);
            return diet;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error through Json parsing plan. Model payload: {returnedPlan}", ex);
        }
    }

    protected virtual async Task<Diets> UpdatePlanForPrompt(
        int id,
        int age,
        decimal actualWeight,
        decimal actualHeight,
        SexEnum sex,
        DietType dietType,
        decimal caloricDemand,
        decimal previousWeight,
        decimal previousHeight,
        decimal previousCaloricDemand,
        GoalType goalType,
        ActivityLevel activityLevel,
        int mealsPerDay,
        IReadOnlyCollection<string>? allergies,
        IReadOnlyCollection<string>? excludedIngredients)
    {
        string promptBuilder = DietPrompts.UpdatePlanPrompt(id) + PreferencePromptSection();

        var kernelArguments = new KernelArguments
        {
            ["age"] = age,
            ["weight"] = actualWeight,
            ["height"] = actualHeight,
            ["sex"] = sex.ToString(),
            ["dietType"] = dietType.ToString(),
            ["caloricDemand"] = caloricDemand.ToString(),
            ["previousWeight"] = previousWeight.ToString(),
            ["previousHeight"] = previousHeight.ToString(),
            ["previousCaloricDemand"] = previousCaloricDemand.ToString(),
            ["goalType"] = goalType.ToString(),
            ["activityLevel"] = activityLevel.ToString(),
            ["mealsPerDay"] = mealsPerDay.ToString(),
            ["allergies"] = FormatPreferenceList(allergies),
            ["excludedIngredients"] = FormatPreferenceList(excludedIngredients),
        };

        var returnedPlan = await InvokePromptWithTimeoutAsync(promptBuilder, kernelArguments);
        if (string.IsNullOrWhiteSpace(returnedPlan))
        {
            throw new Exception("Model returned empty response");
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(returnedPlan);
            var diet = ParseDiet(
                jsonDocument.RootElement,
                fallbackHeight: actualHeight,
                fallbackDietType: dietType);
            ApplyPreferenceSnapshot(diet, goalType, activityLevel, mealsPerDay, allergies, excludedIngredients);
            return diet;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error through Json parsing plan. Model payload: {returnedPlan}", ex);
        }
    }

    private async Task<string?> InvokePromptWithTimeoutAsync(string prompt, KernelArguments kernelArguments)
    {
        try
        {
            var functionResult = await _kernelWrapper
                .InvokePromptAsync(prompt, kernelArguments)
                .AsTask()
                .WaitAsync(ModelExecutionTimeout);

            return functionResult?.ToString();
        }
        catch (TimeoutException)
        {
            throw new TimeoutException("Execution model timeout, try again later.");
        }
    }

    private static string PreferencePromptSection() =>
        @"
            Meal preference constraints:
            - GoalType: {{$goalType}}
            - ActivityLevel: {{$activityLevel}}
            - MealsPerDay: {{$mealsPerDay}}
            - Allergies: {{$allergies}}. Treat allergies as strict safety exclusions.
            - ExcludedIngredients: {{$excludedIngredients}}
            ";

    private static string FormatPreferenceList(IReadOnlyCollection<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return "None";
        }

        var nonEmptyValues = values.Where(value => !string.IsNullOrWhiteSpace(value));
        return string.Join(", ", nonEmptyValues);
    }

    private static void ApplyPreferenceSnapshot(
        Diets diet,
        GoalType goalType,
        ActivityLevel activityLevel,
        int mealsPerDay,
        IReadOnlyCollection<string>? allergies,
        IReadOnlyCollection<string>? excludedIngredients)
    {
        diet.GoalType = goalType;
        diet.ActivityLevel = activityLevel;
        diet.MealsPerDay = mealsPerDay;
        diet.Allergies = allergies?.ToList() ?? [];
        diet.ExcludedIngredients = excludedIngredients?.ToList() ?? [];
    }

    private static Diets ParseDiet(
        JsonElement rootElement,
        decimal fallbackHeight,
        DietType fallbackDietType)
    {
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Model response is not a JSON object.");
        }

        return new Diets
        {
            Id = GetRequiredInt(rootElement, "Id"),
            DietName = GetRequiredString(rootElement, "DietName"),
            Description = GetRequiredString(rootElement, "Description"),
            Age = GetRequiredInt(rootElement, "Age"),
            ForWeight = GetRequiredDecimal(rootElement, "ForWeight"),
            ForHeight = GetDecimalOrDefault(rootElement, "ForHeight", fallbackHeight),
            CaloricValue = GetRequiredDecimal(rootElement, "CaloricValue"),
            ForSex = GetRequiredEnum<SexEnum>(rootElement, "ForSex"),
            DietType = GetEnumOrDefault(rootElement, "DietType", fallbackDietType)
        };
    }

    private static string GetRequiredString(JsonElement rootElement, string propertyName)
    {
        if (!rootElement.TryGetProperty(propertyName, out var property))
        {
            throw new JsonException($"Missing required property '{propertyName}'.");
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? throw new JsonException($"Property '{propertyName}' is null.")
            : property.ToString();
    }

    private static int GetRequiredInt(JsonElement rootElement, string propertyName)
    {
        if (!rootElement.TryGetProperty(propertyName, out var property))
        {
            throw new JsonException($"Missing required property '{propertyName}'.");
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
        {
            return number;
        }

        var raw = property.ToString();
        var match = Regex.Match(raw, @"-?\d+");
        if (!match.Success || !int.TryParse(match.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new JsonException($"Property '{propertyName}' with value '{raw}' cannot be parsed as int.");
        }

        return parsed;
    }

    private static decimal GetRequiredDecimal(JsonElement rootElement, string propertyName)
    {
        if (!rootElement.TryGetProperty(propertyName, out var property))
        {
            throw new JsonException($"Missing required property '{propertyName}'.");
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number))
        {
            return number;
        }

        var raw = property.ToString();
        var normalized = raw.Replace(',', '.');
        var match = Regex.Match(normalized, @"-?\d+(\.\d+)?");
        if (!match.Success || !decimal.TryParse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new JsonException($"Property '{propertyName}' with value '{raw}' cannot be parsed as decimal.");
        }

        return parsed;
    }

    private static decimal GetDecimalOrDefault(JsonElement rootElement, string propertyName, decimal defaultValue)
    {
        if (!rootElement.TryGetProperty(propertyName, out _))
        {
            return defaultValue;
        }

        return GetRequiredDecimal(rootElement, propertyName);
    }

    private static TEnum GetRequiredEnum<TEnum>(JsonElement rootElement, string propertyName)
        where TEnum : struct
    {
        if (!rootElement.TryGetProperty(propertyName, out var property))
        {
            throw new JsonException($"Missing required property '{propertyName}'.");
        }

        var raw = property.ToString();
        if (Enum.TryParse<TEnum>(raw, true, out var parsed))
        {
            return parsed;
        }

        throw new JsonException($"Property '{propertyName}' with value '{raw}' cannot be parsed as {typeof(TEnum).Name}.");
    }

    private static TEnum GetEnumOrDefault<TEnum>(JsonElement rootElement, string propertyName, TEnum defaultValue)
        where TEnum : struct
    {
        if (!rootElement.TryGetProperty(propertyName, out _))
        {
            return defaultValue;
        }

        return GetRequiredEnum<TEnum>(rootElement, propertyName);
    }

    private static bool PreferenceListsEqual(
        IReadOnlyCollection<string>? actual,
        IReadOnlyCollection<string>? expected)
    {
        var normalizedActual = NormalizePreferences(actual);
        var normalizedExpected = NormalizePreferences(expected);

        return normalizedActual.SequenceEqual(normalizedExpected, StringComparer.OrdinalIgnoreCase);
    }

    private static string[] NormalizePreferences(IReadOnlyCollection<string>? values)
    {
        return (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
