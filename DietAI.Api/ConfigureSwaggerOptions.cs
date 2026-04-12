using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DietAI.Api;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
            options.SwaggerDoc(
                description.GroupName,
                BuildInfoForApiVersion(description));
    }

    private OpenApiInfo BuildInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "Dietetic AI API",
            Version = description.ApiVersion.ToString(),
            Description = "Diet plan AI API provider.",
            Contact = new OpenApiContact
            {
                Name = "user123",
                Email = "contoso@contoso.com"
            }
        };
        
        if(description.IsDeprecated)
            info.Description += " This API version has been deprecated.";
        
        return info;
    }
}