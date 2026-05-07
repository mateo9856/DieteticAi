namespace DieteticAI.UI.Services.Login.Models;

public sealed class LoginResponse
{
    public required string UserId { get; init; }

    public required string AccessToken { get; init; }

    public DateTime ExpiresAtUtc { get; init; }
}
