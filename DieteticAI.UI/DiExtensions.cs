using DieteticAI.UI.Services.AiPlanSender.Abstractions;
using DieteticAI.UI.Services.AiPlanSender.Implementations;
using DieteticAI.UI.Services.Login.Abstractions;
using DieteticAI.UI.Services.Login.Implementations;

namespace DieteticAI.UI;

public static class DiExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRequiredServices()
        {
            services.AddScoped<IAiPlanSender, AiPlanSenderService>();
            services.AddScoped<IUserLoginService, UserLoginService>();
            return services;
        }
    }
}
