using DietAI.Api.Services.Login.Abstractions;
using DietAI.Api.Services.Login.Models;
using DietAI.Api.Services.Login.Requests;

namespace DietAI.Api.Services.Login.Implementations;

public class LoginService(JwtTokenService jwtTokenService) : ILoginService
{
    private const string DefaultUsername = "admin";
    private const string DefaultPassword = "password";

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isValidUser = string.Equals(request.Username, DefaultUsername, StringComparison.Ordinal)
            && string.Equals(request.Password, DefaultPassword, StringComparison.Ordinal);

        if (!isValidUser)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var userId = Guid.NewGuid().ToString();
        var accessToken = jwtTokenService.GenerateToken(userId);

        return Task.FromResult(new LoginResponse
        {
            UserId = userId,
            AccessToken = accessToken,
            ExpiresAtUtc = jwtTokenService.GetTokenExpiration(accessToken)
        });
    }
}
