using Microsoft.SemanticKernel;

namespace DieteticAi.Tools.Wrappers;

public interface IKernelWrapper
{
    KernelFunction CreateFunctionFromPrompt(string prompt);
}