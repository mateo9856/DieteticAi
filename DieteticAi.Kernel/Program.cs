using DietAi.AiKernel;
using DietAI.AiKernel;
using DietAI.RabbitServer.Abstractions;
using DietAI.RabbitServer.Implementations.ReceiverService;
using DietAI.RabbitServer.Implementations.SenderService;
using DieteticAi.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        
        var aiConfig = new AiModelSelector(kernelConfig);
        var kernel = aiConfig.BuildKernel();
        
        if (kernelConfig.TestMode)
        {
            await new DietSimulator(kernel).Run();
            return;
        }

        ServiceCollection services = new();
        services.AddTransient<IReceiveService, ReceiverService>();
        services.AddTransient<ISenderService, SenderService>();
        services.AddSingleton<DietConcurrentRunner>();
        services.AddLogging(builder =>
        {
            builder.AddEventSourceLogger();
        });
        
        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<DietConcurrentRunner>().Run();
        
    }    
}