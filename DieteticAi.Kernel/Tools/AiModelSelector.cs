using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace DieteticAi.Tools;

public class AiModelSelector
{
    private const string OllamaAI = "Ollama";
    private const string GeminiAI = "Gemini";
    
    private string SelectedAi;
    
    private readonly KernelConfiguration _configuration;

    public AiModelSelector(KernelConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Kernel BuildKernel()
    {
        var llmConfig = GetSelectedAiModel();
        if (string.IsNullOrEmpty(llmConfig.llmModel) || string.IsNullOrEmpty(llmConfig.host))
            throw new ArgumentNullException("Check Application settings! Application is incorrect parametrized!");
        
        var kernel = Kernel.CreateBuilder();

        if (SelectedAi == GeminiAI)
        {
            return kernel.AddGoogleAIGeminiChatCompletion(
                    modelId: llmConfig.llmModel, llmConfig.host)
                .Build();
        }

        if (SelectedAi == OllamaAI)
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
        SelectedAi = _configuration.SelectedModelType
                         ?? OllamaAI;

        return SelectedAi switch
        {
            GeminiAI => (_configuration.Gemini.ApiKey, _configuration.Gemini.LlmModel),
            OllamaAI => (_configuration.Ollama.Connection, _configuration.Ollama.LlmModel),
            _ => throw new ArgumentNullException("Incorrect select model, check application settings!")
        };
    }

}