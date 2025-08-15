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
        
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        //Agent
        var history = new ChatHistory();
        
        Console.WriteLine("If you leave app type 'exit' or nothing :)");
        
        while (true)
        {
            var userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput) || userInput == "exit")
            {
                Console.WriteLine("Exiting...");
                break;
            }
            
            history.AddUserMessage(userInput);
            Console.WriteLine($"YOU > {userInput}");

            var result = await chatService.GetChatMessageContentAsync(history,
                promptSettings,
                kernel);
            
            Console.WriteLine($"ASSISTANT > {result}");
            
            history.AddAssistantMessage(result.Content);
        }
        
    }    
}