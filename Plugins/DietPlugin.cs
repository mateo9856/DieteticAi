using System.ComponentModel;
using System.Text;
using DieteicAi.Models;
using Microsoft.SemanticKernel;

namespace DieteticAi.Plugins;

public class DietPlugin
{
    private List<Diets> _diets = new List<Diets>();

    [KernelFunction("add_diet_to_list")]
    [Description("Adds Generated Plan to List if it was not find.")]
    public Task AddDietPlanToList(
        [Description("Plan model with Age, CurrentWeight, Sex, Target Caloric etc.")] Diets diets)
    {
        _diets.Add(diets);
        
        return Task.CompletedTask;
    }

    [KernelFunction("get_diet_if_exists")]
    [Description("Find suggested plan if it exist on diets list.")]
    public string GetPlanFromList(
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
        
        return findingPlan is not null ? findingPlan.Description : "NOT FOUND";
    }
}