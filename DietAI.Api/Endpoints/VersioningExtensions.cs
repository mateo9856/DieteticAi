using Asp.Versioning.Conventions;

namespace DietAI.Api.Endpoints;

public static class VersioningExtensions
{
    extension(WebApplication app)
    {
        public RouteGroupBuilder MapGroupWithVersion(string groupEndpoint, string groupTag, int mainNr, int secondNr = 0)
        {
            var versionSet = app.NewApiVersionSet()
                .HasApiVersion(mainNr, secondNr)
                .ReportApiVersions()
                .Build();

            return app.MapGroup(groupEndpoint)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(mainNr, secondNr)
                .WithTags(groupTag);
        }   
    }
}