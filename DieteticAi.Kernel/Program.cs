using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DieteticAi.Models;
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