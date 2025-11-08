using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using DieteticAi.Models;
using Microsoft.SemanticKernel;

namespace DieteticAi.Plugins;

public class DietPlugin
{
    private readonly IList<Diets> _diets;
    private readonly Kernel _kernel;

    public DietPlugin(IList<Diets> diets, Kernel kernel)
    {
        _diets = diets;
        _kernel = kernel;
    }
    
    [KernelFunction("get_diet_or_generate")]
    [Description("Find suggested plan if it exist, otherwise generate new plan.")]
    public string GetPlanFromListOrPrompt(
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
            return findingPlan.Description;
        }

        var parametrizeCaloric = currentCaloricDemand == 0 ? (decimal?)null : currentCaloricDemand;
        var generatedDiet = GenerateNewPlanForPrompt(age, currentWeight, currentHeight, sex, dietType, parametrizeCaloric);
        _diets.Add(generatedDiet);
        
        return generatedDiet.Description;
    }

    protected virtual Diets GenerateNewPlanForPrompt(int age, decimal currentWeight, decimal currentHeight, SexEnum sex, DietType dietType, decimal? caloric)
    {
        string promptBuilder = @"
        You are a diet planner.
        Generate a monthly diet plan in **JSON format** like this:
        {
          ""Id"": ""..."",
          ""DietName"": ""..."",
          ""Description"": ""..."",
          ""Age"": ""..."",
          ""ForWeight"": ""..."",
          ""CaloricValue"": ""..."",
          ""ForSex"": ""..."",

        }
        Id must have a value eqaul to: " + (_diets.Count + 1) + @"
        DietName should return basically diet topic name and Description summary plan with calculated value.
        Rest of fields similar to input, only CaloricValue should return calculated daily caloric.

        Constraints:
        - Age: {{age}}
        - Weight: {{weight}} kg
        - Height: {{height}} cm
        - Sex: {{sex}}
        - DietType: {{dietType}}
        ";
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
        var functionResult = _kernel.CreateFunctionFromPrompt(promptBuilder).InvokeAsync(kernelArguments);

        var returnedPlan = functionResult.ToString();
        if (string.IsNullOrWhiteSpace(returnedPlan))
        {
            throw new Exception("Model returned empty response");
        }

        try
        {
            JsonDocument.Parse(returnedPlan);
            return JsonSerializer.Deserialize<Diets>(returnedPlan) 
                   ?? throw new Exception("Error through deserialize, unexpected error in returned prompt.");
        }
        catch
        {
            throw new Exception("Error through Json parsing plan");
        }

    }
}