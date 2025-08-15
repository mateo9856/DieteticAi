using DieteicAi.Models;
using DieteticAi.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

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

        var promptSettings = new OllamaPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        kernel.Plugins.AddFromType<DietPlugin>("Diets");
        
        Console.WriteLine("Enter your data, first age, next weight and last sex(type Male, Female or Unbinary)");
        
        var age = int.Parse(Console.ReadLine());
        var weight = decimal.Parse(Console.ReadLine());
        var sex = Enum.Parse<SexEnum>(Console.ReadLine());

        var dto = new HumanDataDto
        {
            Age = age,
            ActualWeight = weight,
            Sex = sex
        };
        
        var prompt = $"You are dietietic, please generic me perfect plan for me, Age = {dto.Age}, Weight = {dto.ActualWeight}, Sex = {dto.Sex}. ";
        prompt += "Also return me for Json format: {'description': string, 'dailyCaloric': int}";
        
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        //Agent
        var history = new ChatHistory();
        
        Console.WriteLine("If you leave app type 'exit' or nothing :)");
        
        var userInput = Console.ReadLine();
        
        history.AddUserMessage(userInput);
        Console.WriteLine($"YOU > {userInput}");

        var result = await chatService.GetChatMessageContentAsync(history,
            promptSettings,
            kernel);
            
        Console.WriteLine($"ASSISTANT > {result}");
            
        history.AddAssistantMessage(result.Content);
    }    
}