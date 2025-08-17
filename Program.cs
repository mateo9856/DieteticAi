using DieteicAi.Models;
using DieteticAi.Plugins;
using Microsoft.SemanticKernel;

namespace DieteticAi;

class Program
{ 
    static async Task Main(string[] args)
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion(
                modelId: "llama3",
                endpoint: new Uri("http://localhost:11434")
                ).Build();

        var dietPlugin = new DietPlugin(new List<Diets>(), kernel);
        kernel.Plugins.AddFromObject(dietPlugin, "DietPlugin");
        
        Console.WriteLine("Enter your data, first age, next weight and last sex(type Male, Female or Unbinary)");
        
        var age = int.Parse(Console.ReadLine() ?? "0");
        var weight = decimal.Parse(Console.ReadLine() ?? "0");
        var sex = Enum.Parse<SexEnum>(Console.ReadLine() ?? "Unbinary");

        var dto = new HumanDataDto
        {
            Age = age,
            ActualWeight = weight,
            Sex = sex
        };

        var plan = dietPlugin.GetPlanFromListOrPrompt(dto.Age, dto.ActualWeight, dto.Sex);
        
        Console.WriteLine($"Generated plan: \n{plan}");

    }    
}