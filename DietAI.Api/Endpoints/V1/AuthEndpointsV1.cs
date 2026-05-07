using DietAI.Api.Services.Login.Models;
using DietAI.Api.Services.Login.Requests;
using DietAI.Api.Commands.Auth.Login;
using MediatR;

namespace DietAI.Api.Endpoints.V1;

public static class AuthEndpointsV1
{
    public static IEndpointRouteBuilder MapAuthEndpointsV1(this WebApplication app)
    {
        var group = app.MapGroupWithVersion("/v1/auth", "Auth", 1);
        
        group.MapPost("/login", async (
                LoginRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
                await HandleLoginAsync(request, mediator, cancellationToken))
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
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password
            };

            var response = await mediator.Send(command, cancellationToken);
            return Results.Ok(response);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
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
}
