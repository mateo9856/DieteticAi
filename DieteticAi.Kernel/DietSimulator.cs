using DietAI.AiKernel.Models.DTOs;
using DieteticAi.Models;
using DieteticAi.Plugins;

namespace DietAi.AiKernel;

public class DietSimulator
{
    private readonly DietPlugin _dietPlugin;

    public DietSimulator(DietPlugin dietPlugin)
    {
        _dietPlugin = dietPlugin;
    }

    public async Task Run(CancellationToken  cancellationToken = default)
    {
        var dto = new HumanDataDto
        {
            Age = 28,
            ActualWeight = 78,
            ActualHeight = 180,
            Sex = SexEnum.Unbinary,
            CaloricDemand = 0,
            DietType = DietType.Standard,
        };

        var plan = await _dietPlugin.GetPlanFromListOrPrompt(
            dto.Age,
            dto.ActualWeight,
            dto.ActualHeight,
            dto.CaloricDemand,
            dto.Sex,
            dto.DietType);

        Console.WriteLine($"Generated plan: \n{plan.Description}");
    }
}
