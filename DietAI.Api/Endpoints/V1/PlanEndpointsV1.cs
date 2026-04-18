using System.ComponentModel.DataAnnotations;
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
            .Produces<Diets>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status504GatewayTimeout);

        return app;
    }

    private static async Task<IResult> HandleSendPlanAsync(
        SendPlanRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.Request.Headers["X-User-Id"].FirstOrDefault()
            ?? httpContext.User.Identity?.Name;

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
        var userId = httpContext.Request.Headers["X-User-Id"].FirstOrDefault()
            ?? httpContext.User.Identity?.Name;

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
}
