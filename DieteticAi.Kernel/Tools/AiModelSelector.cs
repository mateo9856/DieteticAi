using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace DieteticAi.Tools;

public class AiModelSelector
{
    private const string _defaultAi = "Ollama";

    private string SelectedAi;
    
    private readonly IConfiguration _configuration;

    public AiModelSelector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Kernel BuildKernel()
    {
        var llmConfig = GetSelectedAiModel();
        if (string.IsNullOrEmpty(llmConfig.llmModel) || string.IsNullOrEmpty(llmConfig.host))
            throw new ArgumentNullException("Check Application settings! Application is incorrect parametrized!");
        
        var kernel = Kernel.CreateBuilder();

        if (SelectedAi == "Gemini")
        {
            return kernel.AddGoogleAIGeminiChatCompletion(
                    modelId: llmConfig.llmModel, llmConfig.host)
                .Build();
        }

        if (SelectedAi == "Ollama")
        {
            return kernel.AddOllamaChatCompletion(
                modelId: llmConfig.llmModel,
                endpoint: new Uri(llmConfig.host)
            ).Build();
        }

        throw new ArgumentNullException("Incorrect select model, check application settings!");
    }

    private (string? host, string? llmModel) GetSelectedAiModel()
    {
        SelectedAi = _configuration["SelectedModelType"] 
                         ?? _defaultAi;

        if (SelectedAi == "Gemini")
            return (_configuration["Gemini:ApiKey"], _configuration["Gemini:LlmModel"]);

        if (SelectedAi == "Ollama")
            return (_configuration["Ollama:Connection"], _configuration["Ollama:LlmModel"]);
        
        throw new ArgumentNullException("Incorrect select model, check application settings!");
    }

}