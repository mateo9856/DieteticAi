using DietAI.RabbitServer.Abstractions;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using DietAI.RabbitServer.Implementations.RabbitConnection;
using DietAI.RabbitServer.Implementations.ReceiverService;
using DietAI.RabbitServer.Implementations.SenderService;
using DieteticAI.UI.Services.AiPlanSender.Abstractions;
using DieteticAI.UI.Services.AiPlanSender.Implementations;

namespace DieteticAI.UI;

public static class DiExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRequiredServices()
        {
            services.AddTransient<IRabbitConnectionFactory, RabbitConnectionFactory>();
            services.AddTransient<ITopicFactory, TopicFactory>();
            services.AddTransient<IReceiveService, ReceiverService>();
            services.AddTransient<ISenderService, SenderService>();
            services.AddScoped<IAiPlanSender, AiPlanSenderService>();
            return services;
        }
    }
}
