using System.ComponentModel;
using System.Text.Json;
using DieteicAi.Models;
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
        [Description("Sex of the Person (Male/Female/Unbinary)")]
        SexEnum sex
    )
    {
        var ageCorrect = (int a) => age >= 15 && (age - a <= 2);
        var weightCorrect = (decimal b) => (currentWeight - b <= 5 || b + 5 <= currentWeight);

        var findingPlan = _diets.FirstOrDefault(d => 
            ageCorrect(d.Age) 
            && weightCorrect(d.ForWeight)
            && d.ForSex == sex);

        if (findingPlan is not null)
        {
            return findingPlan.Description;
        }
        
        var generatedDiet = GenerateNewPlanForPrompt(age, currentWeight, sex);
        _diets.Add(generatedDiet);
        
        return generatedDiet.Description;
    }

    protected virtual Diets GenerateNewPlanForPrompt(int age, decimal currentWeight, SexEnum sex)
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
        - Sex: {{sex}}
        ";

        var functionResult = _kernel.CreateFunctionFromPrompt(promptBuilder).InvokeAsync(new KernelArguments
        {
            ["age"] = age,
            ["weight"] = currentWeight,
            ["sex"] = sex.ToString()
        });

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