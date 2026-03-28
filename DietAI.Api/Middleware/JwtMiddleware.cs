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
            if (_jwtTokenService.ValidateToken(token))
            {
                // Token is valid, attach user info to context if needed
                var expiration = _jwtTokenService.GetTokenExpiration(token);
                context.Items["TokenExpiration"] = expiration;
            }
            else
            {
                // Token is invalid, return 401
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