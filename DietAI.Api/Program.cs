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
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using DietAI.Api;
using DietAI.Api.Endpoints.V1;

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
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version")
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";            // e.g. v1, v2
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddTransient<IRabbitConnectionFactory, RabbitConnectionFactory>();
builder.Services.AddTransient<ITopicFactory, TopicFactory>();
builder.Services.AddTransient<IReceiveService, ReceiverService>();
builder.Services.AddTransient<ISenderService, SenderService>();
builder.Services.AddScoped<TopicManager>();
builder.Services.AddScoped<IAiPlanSender, AiPlanSenderService>();
builder.Services.AddScoped<ILoginService, LoginService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var appVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in appVersionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Plan API {description.GroupName.ToUpperInvariant()}"
            );
        }
 
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseJwtMiddleware();

app.MapAuthEndpointsV1();
app.MapPlanEndpointsV1();

app.Run();