using System.ComponentModel.DataAnnotations;
using DietAI.Api.Services.AiPlanSender.Abstractions;
using DietAI.Api.Services.AiPlanSender.Models;
using DietAI.Api.Services.AiPlanSender.Requests;

namespace DietAI.Api.Endpoints;

public static class PlanEndpoints
{
    public static IEndpointRouteBuilder MapPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plan")
            .WithTags("Plan");

        group.MapPost("/send", async (
                SendPlanRequest request,
                HttpContext httpContext,
                IAiPlanSender planSender,
                CancellationToken cancellationToken) =>
                await HandleRequestAsync(
                    request,
                    httpContext,
                    userId => planSender.SendPlanRequestAsync(userId, request, cancellationToken)))
            .WithName("SendPlanRequest")
            .WithSummary("Send a new diet plan request")
            .Produces<Diets>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status504GatewayTimeout);

        group.MapPost("/update", async (
                SendUpdatePlanRequest request,
                HttpContext httpContext,
                IAiPlanSender planSender,
                CancellationToken cancellationToken) =>
                await HandleRequestAsync(
                    request,
                    httpContext,
                    userId => planSender.SendPlanUpdateAsync(userId, request, cancellationToken)))
            .WithName("SendPlanUpdate")
            .WithSummary("Send an update for an existing diet plan")
            .Produces<Diets>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status504GatewayTimeout);

        return app;
    }

    private static async Task<IResult> HandleRequestAsync<TRequest>(
        TRequest request,
        HttpContext httpContext,
        Func<string, Task<Diets>> handler)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var userId = httpContext.Request.Headers["X-User-Id"].FirstOrDefault()
            ?? httpContext.User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var result = await handler(userId);
            return Results.Ok(result);
        }
        catch (TimeoutException ex)
        {
            return Results.Problem(
                title: "Diet plan request timed out",
                detail: ex.Message,
                statusCode: StatusCodes.Status504GatewayTimeout);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to send diet plan request",
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
