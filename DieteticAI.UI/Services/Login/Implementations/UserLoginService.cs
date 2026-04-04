using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DieteticAI.UI.Services.Login.Abstractions;
using DieteticAI.UI.Services.Login.Models;
using DieteticAI.UI.Services.Login.Requests;
using DieteticAI.UI.Tools;

namespace DieteticAI.UI.Services.Login.Implementations;

public class UserLoginService(HttpClient httpClient, SessionManager sessionManager) : IUserLoginService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(BuildErrorMessage(response.StatusCode, details));
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize login response.");

        sessionManager.UserId = loginResponse.UserId;
        sessionManager.AccessToken = loginResponse.AccessToken;
        sessionManager.TokenExpiresAt = loginResponse.ExpiresAtUtc;

        return loginResponse;
    }

    public void ClearSession() => sessionManager.Clear();

    private static string BuildErrorMessage(HttpStatusCode statusCode, string? details)
    {
        if (!string.IsNullOrWhiteSpace(details))
        {
            try
            {
                using var document = JsonDocument.Parse(details);
                if (document.RootElement.TryGetProperty("detail", out var detailElement)
                    && !string.IsNullOrWhiteSpace(detailElement.GetString()))
                {
                    return detailElement.GetString()!;
                }

                if (document.RootElement.TryGetProperty("title", out var titleElement)
                    && !string.IsNullOrWhiteSpace(titleElement.GetString()))
                {
                    return titleElement.GetString()!;
                }
            }
            catch (JsonException)
            {
                return details;
            }
        }

        return statusCode == HttpStatusCode.Unauthorized
            ? "Invalid username or password."
            : "Login failed. Please try again.";
    }
}
