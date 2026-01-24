using DietAI.Kernel.Models;
using DieteticAi.Models;
using DieteticAi.Plugins;
using DieteticAi.Tools;
using DieteticAi.Tools.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace DieteticAi;

class Program
{ 
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var kernelConfig = new KernelConfiguration();
        configuration.Bind(kernelConfig);

        if (!kernelConfig.TestMode)
        {
            //TODO: prepare concurrent process to consume DietPlans
            return;
        }
        
        var aiConfig = new AiModelSelector(kernelConfig);
        var kernel = aiConfig.BuildKernel();
        
        var dietPlugin = new DietPlugin(new List<Diets>(), new KernelWrapper(kernel));
        kernel.Plugins.AddFromObject(dietPlugin, "DietPlugin");
        
        Console.WriteLine("Enter your data, first age, next weight, sex(type Male, Female or Unbinary)");
        Console.WriteLine("Then type DietType, your height and caloric demand");
        
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

        var plan = dietPlugin.GetPlanFromListOrPrompt(dto.Age, dto.ActualWeight, dto.ActualHeight, dto.CaloricDemand, dto.Sex, dto.DietType);
        
        Console.WriteLine($"Generated plan: \n{plan}");

    }    
}