using System.ComponentModel.DataAnnotations;
using DietAI.Api.Services.Login.Abstractions;
using DietAI.Api.Services.Login.Models;
using DietAI.Api.Services.Login.Requests;

namespace DietAI.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", async (
                LoginRequest request,
                ILoginService loginService,
                CancellationToken cancellationToken) =>
                await HandleLoginAsync(request, loginService, cancellationToken))
            .WithName("LoginUser")
            .WithSummary("Authenticate a user and create a UI session")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> HandleLoginAsync(
        LoginRequest request,
        ILoginService loginService,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        try
        {
            var response = await loginService.LoginAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                title: "Authentication failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Login failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static Dictionary<string, string[]> Validate<T>(T request)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request!);

        var isValid = Validator.TryValidateObject(
            request!,
            validationContext,
            validationResults,
            validateAllProperties: true);

        if (isValid)
        {
            return new Dictionary<string, string[]>();
        }

        return validationResults
            .SelectMany(
                result => result.MemberNames.DefaultIfEmpty(string.Empty),
                (result, memberName) => new { memberName, result.ErrorMessage })
            .GroupBy(item => string.IsNullOrWhiteSpace(item.memberName) ? "request" : item.memberName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.ErrorMessage ?? "Invalid value").ToArray());
    }
}
