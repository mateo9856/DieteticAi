using DietAI.Api.Commands.Auth.KeycloakLogin;
using DietAI.Api.Services.Login.Models;
using DietAI.Api.Services.Login.Requests;
using DietAI.Api.Commands.Auth.Login;
using DietAI.Api.Services.Keycloak.Abstractions;
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

        group.MapGet("/keycloak/login", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new KeycloakLoginCommand(), cancellationToken);
                httpContext.Response.Cookies.Append(
                    "DietAI.Keycloak.State",
                    result.State,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = httpContext.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        MaxAge = TimeSpan.FromMinutes(10)
                    });

                return Results.Redirect(result.LoginUri.ToString());
            })
            .WithName("LoginWithKeycloak")
            .WithSummary("Redirect to Keycloak login")
            .Produces(StatusCodes.Status302Found);

        group.MapGet("/keycloak/callback", async (
                string? code,
                string? state,
                string? error,
                HttpContext httpContext,
                IKeycloakLoginService keycloakLoginService,
                CancellationToken cancellationToken) =>
                await HandleKeycloakCallbackAsync(code, state, error, httpContext, keycloakLoginService, cancellationToken))
            .WithName("CompleteKeycloakLogin")
            .WithSummary("Complete Keycloak login and create a UI session")
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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

    private static async Task<IResult> HandleKeycloakCallbackAsync(
        string? code,
        string? state,
        string? error,
        HttpContext httpContext,
        IKeycloakLoginService keycloakLoginService,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            return Results.Problem(
                title: "Keycloak login failed",
                detail: error,
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var expectedState = httpContext.Request.Cookies["DietAI.Keycloak.State"];
        httpContext.Response.Cookies.Delete("DietAI.Keycloak.State");

        if (string.IsNullOrWhiteSpace(code)
            || string.IsNullOrWhiteSpace(state)
            || string.IsNullOrWhiteSpace(expectedState)
            || !string.Equals(state, expectedState, StringComparison.Ordinal))
        {
            return Results.Problem(
                title: "Invalid Keycloak callback",
                detail: "The login callback is missing required data or has an invalid state.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var response = await keycloakLoginService.CompleteLoginAsync(code, cancellationToken);
            return Results.Redirect(keycloakLoginService.BuildUiRedirectUri(response).ToString());
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                title: "Keycloak login failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized);
        }
    }
}
