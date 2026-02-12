using DietAI.Kernel.Models;
using DieteticAi.Models;
using DieteticAi.Plugins;
using DieteticAi.Tools.Wrappers;
using Microsoft.SemanticKernel;

namespace DietAi.AiKernel;

public class DietSimulator
{
    private DietPlugin _dietPlugin;

    public DietSimulator(Kernel kernel)
    {
        _dietPlugin = new DietPlugin(new List<Diets>(), new KernelWrapper(kernel));
        kernel.Plugins.AddFromObject(_dietPlugin, "DietPlugin");
    }

    public Task Run(CancellationToken  cancellationToken = default)
    {
        Console.WriteLine("Enter your data, first age, next weight, sex(type Male, Female or Unbinary)");
        Console.WriteLine("Then type DietType, your height and caloric demand");

        try
        {
            var age = int.Parse(Console.ReadLine() ?? "0");
            var weight = decimal.Parse(Console.ReadLine() ?? "0");
            var sex = Enum.Parse<SexEnum>(Console.ReadLine() ?? "Unbinary");
            var dietType = Enum.Parse<DietType>(Console.ReadLine() ?? "Standard");
            var height = decimal.Parse(Console.ReadLine() ?? "0");
            var caloricDemand = decimal.Parse(Console.ReadLine() ?? "0");

            var dto = new HumanDataDto
            {
                Age = age,
                ActualWeight = weight,
                ActualHeight = height,
                Sex = sex,
                CaloricDemand = caloricDemand,
                DietType = dietType,
            };

            var plan = _dietPlugin.GetPlanFromListOrPrompt(dto.Age, dto.ActualWeight, dto.ActualHeight,
                dto.CaloricDemand, dto.Sex, dto.DietType);

            Console.WriteLine($"Generated plan: \n{plan.Description}");
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
        
        return Task.CompletedTask;
    }
}