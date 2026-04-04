using DieteticAI.UI;
using DieteticAI.UI.Tools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddRequiredServices();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute) });
builder.Services.AddSingleton<SessionManager>();

await builder.Build().RunAsync();