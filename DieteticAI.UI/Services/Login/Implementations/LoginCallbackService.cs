using DieteticAI.UI.Extensions;
using DieteticAI.UI.Services.Login.Abstractions;
using DieteticAI.UI.Services.Login.Models;

namespace DieteticAI.UI.Services.Login.Implementations;

public sealed class LoginCallbackService(IUserLoginService userLoginService) : ILoginCallbackService
{
    public bool TryCompleteLogin(Uri currentUri)
    {
        var query = currentUri.ParseQueryString();

        if (!query.TryGetValue("accessToken", out var accessToken)
            || !query.TryGetValue("userId", out var userId)
            || !query.TryGetValue("expiresAtUtc", out var expiresAtUtc)
            || !DateTime.TryParse(expiresAtUtc, out var expiresAt))
        {
            return false;
        }

        userLoginService.CompleteLogin(new LoginResponse
        {
            UserId = userId,
            Email = query.GetValueOrDefault("email"),
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAt.ToUniversalTime()
        });

        return true;
    }
}
