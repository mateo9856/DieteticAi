using System.ComponentModel;
using System.Text;
using DieteicAi.Models;
using Microsoft.SemanticKernel;

namespace DieteticAi.Plugins;

public class DietPlugin
{
    private List<Diets> diets = new List<Diets>();

    [KernelFunction("get_list_diets")]
    [Description("Gets a list of all diet's what can be suggest")]
    public string GetDiets()
    {
        var builder = new StringBuilder();
        builder.AppendLine("List of possible diets:");
        foreach (var diet in diets)
        {
            builder.AppendLine(diet.ToString());
        }
        
        return builder.ToString();
    }

    [KernelFunction("PrepareDietPlan")]
    [Description("Preppare diet plan base for arguments.")]
    public string PrepareDietPlan(
        [Description] int approxCaloric,
        [Description] SexEnum sex)
    {
        Diets selectedDiet;
        var decidedPlan = diets
            .Where(d => d.CaloricValue >= approxCaloric 
                        || d.CaloricValue <= approxCaloric
                        && d.ForSex.Contains(sex));

        var planCount = decidedPlan.Count();
        if (planCount == 0)
            return "I'm sorry, we don't have plan for your prequisitions";
        
        if (planCount <= 2)
        {
            selectedDiet = decidedPlan.FirstOrDefault();
        }
        else
        {
            selectedDiet = decidedPlan.OrderBy(d => d.CaloricValue).ToList()[planCount / 2];
        }

        return $"There's your plan: \n {selectedDiet.Description}";

    }
}