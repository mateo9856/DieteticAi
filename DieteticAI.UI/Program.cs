using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DieteticAI.UI.Services.AiPlanSender.Abstractions;
using DieteticAI.UI.Services.AiPlanSender.Implementations;
using DieteticAI.UI.Tools;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<DieteticAI.UI.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddScoped<IAiPlanSender, AiPlanSenderService>();

await builder.Build().RunAsync();