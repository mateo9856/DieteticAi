using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DietAI.Api.Commands.Plan.History;
using DietAI.Api.Services.AiPlanSender.Models;
using DietAI.Api.Services.AiPlanSender.Requests;
using DietAI.Api.Commands.Plan.SendPlan;
using DietAI.Api.Commands.Plan.UpdatePlan;
using MediatR;

namespace DietAI.Api.Endpoints.V1;

public static class PlanEndpointsV1
{
    public static IEndpointRouteBuilder MapPlanEndpointsV1(this WebApplication app)
    {
        var group = app.MapGroupWithVersion("/v1/plan", "Plan", 1);

        group.MapPost("/send", async (
                SendPlanRequest request,
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
                await HandleSendPlanAsync(request, httpContext, mediator, cancellationToken))
            .WithName("SendPlanRequest")
            .WithSummary("Send a new diet plan request")
            .RequireAuthorization()
            .Produces<Diets>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status504GatewayTimeout);

        group.MapPost("/update", async (
                SendUpdatePlanRequest request,
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
                await HandleUpdatePlanAsync(request, httpContext, mediator, cancellationToken))
            .WithName("SendPlanUpdate")
            .WithSummary("Send an update for an existing diet plan")
            .RequireAuthorization()
            .Produces<Diets>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status504GatewayTimeout);

        group.MapGet("/history", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
                await HandleGetHistoryAsync(httpContext, mediator, cancellationToken))
            .WithName("GetPlanHistory")
            .WithSummary("Get saved diet plans for the current user")
            .RequireAuthorization()
            .Produces<IReadOnlyList<Diets>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/history/{id:int}", async (
                int id,
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
                await HandleGetHistoryDetailAsync(id, httpContext, mediator, cancellationToken))
            .WithName("GetPlanHistoryDetail")
            .WithSummary("Get one saved diet plan for the current user")
            .RequireAuthorization()
            .Produces<Diets>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> HandleSendPlanAsync(
        SendPlanRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContext);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var command = new SendPlanCommand
            {
                UserId = userId,
                Request = request
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
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

    private static async Task<IResult> HandleUpdatePlanAsync(
        SendUpdatePlanRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContext);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var command = new UpdatePlanCommand
            {
                UserId = userId,
                Request = request
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }
        catch (TimeoutException ex)
        {
            return Results.Problem(
                title: "Diet plan update timed out",
                detail: ex.Message,
                statusCode: StatusCodes.Status504GatewayTimeout);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to send diet plan update",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> HandleGetHistoryAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContext);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new GetPlanHistoryQuery { UserId = userId }, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetHistoryDetailAsync(
        int id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContext);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new GetPlanHistoryDetailQuery { UserId = userId, Id = id }, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static string? GetCurrentUserId(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
    }
}
