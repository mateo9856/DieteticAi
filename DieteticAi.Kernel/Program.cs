using DietAi.AiKernel;
using DietAI.AiKernel;
using DietAI.AiKernel.Services;
using DietAI.RabbitServer.Abstractions;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using DietAI.RabbitServer.Implementations.RabbitConnection;
using DietAI.RabbitServer.Implementations.ReceiverService;
using DietAI.RabbitServer.Implementations.SenderService;
using DieteticAi.Models;
using DieteticAi.Plugins;
using DieteticAi.Tools;
using DieteticAi.Tools.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

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
        
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton(kernelConfig);

                services.AddSingleton(serviceProvider =>
                {
                    var aiConfig = new AiModelSelector(serviceProvider.GetRequiredService<KernelConfiguration>());
                    return aiConfig.BuildKernel();
                });
                services.AddSingleton<IKernelWrapper, KernelWrapper>();
                services.AddSingleton<IList<Diets>>(_ => new List<Diets>());
                services.AddSingleton(serviceProvider =>
                {
                    var dietPlugin = new DietPlugin(
                        serviceProvider.GetRequiredService<IList<Diets>>(),
                        serviceProvider.GetRequiredService<IKernelWrapper>());

                    var kernel = serviceProvider.GetRequiredService<Microsoft.SemanticKernel.Kernel>();
                    kernel.Plugins.AddFromObject(dietPlugin, nameof(DietPlugin));

                    return dietPlugin;
                });
                services.AddSingleton<DietSimulator>();

                services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
                services.AddSingleton<ITopicFactory, TopicFactory>();
                services.AddSingleton<TopicManager>();

                services.AddTransient<IReceiveService, ReceiverService>();
                services.AddTransient<ISenderService, SenderService>();

                services.AddTransient<DietService>();

                services.AddHostedService<DietConcurrentRunner>();

                services.AddLogging(builder =>
                {
                    builder.AddEventSourceLogger();
                });
            });

        if (kernelConfig.TestMode)
        {
            using var testHost = hostBuilder.Build();
            await testHost.Services.GetRequiredService<DietSimulator>().Run();
            return;
        }

        var host = hostBuilder.Build();

        await host.RunAsync();
    }    
}
