using Microsoft.SemanticKernel;

namespace DieteticAi.Tools.Wrappers;

public class KernelWrapper : IKernelWrapper
{
    private readonly Kernel _kernel;

    public KernelWrapper(Kernel kernel)
    {
        _kernel = kernel;
    }
    
    public KernelFunction CreateFunctionFromPrompt(string prompt)
        => _kernel.CreateFunctionFromPrompt(prompt);

    public ValueTask<object?> InvokePromptAsync(string prompt, KernelArguments kernelArguments)
        => _kernel.CreateFunctionFromPrompt(prompt).InvokeAsync(kernelArguments);
}