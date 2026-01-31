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

    public string GenerateNewOrGetPlan(int age, decimal weight, decimal height, decimal caloricDemand, SexEnum sex, DietType dietType)
    {
        return _dietPlugin.GetPlanFromListOrPrompt(age, weight, height, caloricDemand, sex, dietType);
    }
    
}