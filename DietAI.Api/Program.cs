using DietAI.Api.Endpoints;
using DietAI.Api.Middleware;
using DietAI.Api.Options;
using DietAI.Api.Services;
using DietAI.Api.Services.AiPlanSender.Abstractions;
using DietAI.Api.Services.AiPlanSender.Implementations;
using DietAI.Api.Services.Login.Abstractions;
using DietAI.Api.Services.Login.Implementations;
using DietAI.Api.Tools;
using DietAI.RabbitServer.Abstractions;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using DietAI.RabbitServer.Implementations.RabbitConnection;
using DietAI.RabbitServer.Implementations.ReceiverService;
using DietAI.RabbitServer.Implementations.SenderService;

const string UiCorsPolicy = "UiClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddTransient<IRabbitConnectionFactory, RabbitConnectionFactory>();
builder.Services.AddTransient<ITopicFactory, TopicFactory>();
builder.Services.AddTransient<IReceiveService, ReceiverService>();
builder.Services.AddTransient<ISenderService, SenderService>();
builder.Services.AddScoped<TopicManager>();
builder.Services.AddScoped<IAiPlanSender, AiPlanSenderService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(UiCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5141", "https://localhost:7284")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(UiCorsPolicy);
app.UseJwtMiddleware();

app.MapGet("/", () => Results.Ok(new { status = "DietAI.Api is running" }));
app.MapAuthEndpoints();
app.MapPlanEndpoints();

app.Run();