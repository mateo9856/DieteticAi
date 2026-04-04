using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using DietAI.Api.Services;

namespace DietAI.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtTokenService _jwtTokenService;

    public JwtMiddleware(RequestDelegate next, JwtTokenService jwtTokenService)
    {
        _next = next;
        _jwtTokenService = jwtTokenService;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            var principal = _jwtTokenService.GetPrincipalFromToken(token);
            if (principal is not null)
            {
                context.User = principal;
                context.Items["TokenExpiration"] = _jwtTokenService.GetTokenExpiration(token);
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid token");
                return;
            }
        }

        await _next(context);
    }
}

// Extension method to add the middleware to the pipeline
public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}