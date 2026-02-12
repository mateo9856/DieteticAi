using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using DieteticAi.Constaints;
using DieteticAi.Models;
using DieteticAi.Tools.Wrappers;
using Microsoft.SemanticKernel;

namespace DieteticAi.Plugins;

public class DietPlugin
{
    private readonly IList<Diets> _diets;
    private readonly IKernelWrapper _kernelWrapper;

    public DietPlugin(IList<Diets> diets, IKernelWrapper kernel)
    {
        _diets = diets;
        _kernelWrapper = kernel;
    }

    [KernelFunction("update_existing_plan")]
    [Description("Define previous plan and then update to new plan details.")]
    public Diets UpdatePlanByPrompt(
        [Description("Actual age of person")] int actualAge,
        [Description("Previous age of person")] int previousAge,
        [Description("Current weight in kg")] decimal currentWeight,
        [Description("Previous weight in kg")] decimal previousWeight,
        [Description("Current height in cm")] decimal currentHeight,
        [Description("Previous height in kg")] decimal previousHeight,
        [Description("Current caloric demand")] decimal currentCaloricDemand,
        [Description("Previous caloric demand")] decimal previousCaloricDemand,
        [Description("Sex of the Person (Male/Female/Unbinary)")] SexEnum sex,
        [Description("Type of diet")]DietType dietType
        )
    {
        var existingPlans = _diets.Where(diet => 
            diet.Age == previousAge 
            && diet.ForWeight == previousWeight
            && diet.ForHeight == previousHeight
            && diet.CaloricValue == previousCaloricDemand
            && diet.ForSex == sex
            && diet.DietType == dietType);

        var existingPlan = existingPlans.FirstOrDefault();
        if (existingPlan is null)
        {
            throw new Exception("Plan does not exist, please generate new plan.");
        }

        var updatedPlan = UpdatePlanForPrompt(existingPlan.Id, actualAge, currentWeight, currentHeight, sex, dietType, currentCaloricDemand, previousWeight, previousHeight, previousCaloricDemand);

        var index = _diets.IndexOf(existingPlan);
        _diets[index] = updatedPlan;

        return updatedPlan;
    }
    
    [KernelFunction("get_diet_or_generate")]
    [Description("Find suggested plan if it exist, otherwise generate new plan.")]
    public Diets GetPlanFromListOrPrompt(
        [Description("Age of the person")] int age,
        [Description("Current weight in kg")] decimal currentWeight,
        [Description("Current height in cm")] decimal currentHeight,
        [Description("Current caloric demand")] decimal currentCaloricDemand,
        [Description("Sex of the Person (Male/Female/Unbinary)")] SexEnum sex,
        [Description("Type of diet")]DietType dietType
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
            && d.DietType == dietType);

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
        var generatedDiet = GenerateNewPlanForPrompt(age, currentWeight, currentHeight, sex, dietType, parametrizeCaloric);
        _diets.Add(generatedDiet);
        
        return generatedDiet;
    }

    protected virtual Diets GenerateNewPlanForPrompt(int age, decimal currentWeight, decimal currentHeight, SexEnum sex, DietType dietType, decimal? caloric)
    {
        string promptBuilder = DietPrompts.CreatePlanPrompt(_diets.Count + 1);
        var kernelArguments = new KernelArguments
        {
            ["age"] = age,
            ["weight"] = currentWeight,
            ["sex"] = sex.ToString(),
            ["dietType"] = dietType.ToString(),
        };

        if (caloric is not null)
        {
            promptBuilder += @"
            - CaloricDemand:  {{caloricDemand}} kcal
            ";
            kernelArguments["caloricDemand"] = caloric.ToString();
        }
        var functionResult = _kernelWrapper.InvokePromptAsync(promptBuilder, kernelArguments);

        var returnedPlan = functionResult.ToString();
        if (string.IsNullOrWhiteSpace(returnedPlan))
        {
            throw new Exception("Model returned empty response");
        }

        try
        {
            JsonDocument.Parse(returnedPlan);
            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<Diets>(returnedPlan, jsonOptions) 
                   ?? throw new Exception("Error through deserialize, unexpected error in returned prompt.");
        }
        catch
        {
            throw new Exception("Error through Json parsing plan");
        }
    }

    protected virtual Diets UpdatePlanForPrompt(int id, int age, decimal actualWeight, decimal actualHeight, SexEnum sex, DietType dietType, decimal caloricDemand, decimal previousWeight, decimal previousHeight, decimal previousCaloricDemand)
    {
        string promptBuilder = DietPrompts.UpdatePlanPrompt(id);

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
            ["previousCaloricDemand"] = previousCaloricDemand.ToString()
        };

        var functionResult = _kernelWrapper.InvokePromptAsync(promptBuilder, kernelArguments);

        var returnedPlan = functionResult.ToString();
        if (string.IsNullOrWhiteSpace(returnedPlan))
        {
            throw new Exception("Model returned empty response");
        }

        try
        {
            JsonDocument.Parse(returnedPlan);
            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<Diets>(returnedPlan, jsonOptions) 
                   ?? throw new Exception("Error through deserialize, unexpected error in returned prompt.");
        }
        catch
        {
            throw new Exception("Error through Json parsing plan");
        }
    }
}