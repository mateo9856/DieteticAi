using DietAI.AiKernel.Models.DTOs;
using DieteticAi.Models;
using DieteticAi.Plugins;

namespace DietAI.AiKernel.Services;

public class DietService
{
    private readonly DietPlugin _dietPlugin;

    public DietService(DietPlugin dietPlugin)
    {
        _dietPlugin = dietPlugin;
    }

    public Task<Diets> GenerateNewOrGetPlan(HumanDataDto dto)
    {
        return _dietPlugin.GetPlanFromListOrPrompt(dto.Age,
            dto.ActualWeight,
            dto.ActualHeight,
            dto.CaloricDemand,
            dto.Sex,
            dto.DietType);
    }

    public Task<Diets> UpdateExistingPlan(UpdateHumanDataDto dto)
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
