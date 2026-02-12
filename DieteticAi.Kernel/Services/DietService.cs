using DietAI.Kernel.Models;
using DieteticAi.Models;
using DieteticAi.Plugins;
using DieteticAi.Tools.Wrappers;

namespace DietAI.AiKernel.Services;

public class DietService
{
    private readonly DietPlugin _dietPlugin;

    public DietService(Microsoft.SemanticKernel.Kernel kernel)
    {
        _dietPlugin = new DietPlugin(new List<Diets>(), new KernelWrapper(kernel));
    }

    public Diets GenerateNewOrGetPlan(HumanDataDto dto)
    {
        return _dietPlugin.GetPlanFromListOrPrompt(dto.Age,
            dto.ActualWeight,
            dto.ActualHeight,
            dto.CaloricDemand,
            dto.Sex,
            dto.DietType);
    }

    public Diets UpdateExistingPlan(UpdateHumanDataDto dto)
    {
        return _dietPlugin.UpdatePlanByPrompt(dto.Age,
            dto.PreviousAge,
            dto.ActualWeight,
            dto.PreviousWeight,
            dto.ActualHeight,
            dto.PreviousHeight,
            dto.CaloricDemand,
            dto.PreviousCaloricDemand,
            dto.Sex,
            dto.DietType);
    }
    
}