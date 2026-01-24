namespace DieteticAi;

public record KernelConfiguration
{
    public string SelectedModelType { get; set; }
    
    public GeminiConfiguration Gemini { get; set; }
    
    public OllamaConfiguration Ollama { get; set; } 
    
    public bool TestMode { get; set; }
}

public record GeminiConfiguration(string ApiKey, string LlmModel);

public record OllamaConfiguration(string Connection, string LlmModel);