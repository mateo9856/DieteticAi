using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using DietAI.Api.Options;
using DietAI.Api.Services.Keycloak.Abstractions;
using DietAI.Api.Services.Keycloak.Models;
using DietAI.Api.Services.Login.Models;
using Microsoft.Extensions.Options;

namespace DietAI.Api.Services.Keycloak.Implementations;

public sealed class KeycloakLoginService(
    HttpClient httpClient,
    JwtTokenService jwtTokenService,
    IOptions<KeycloakOptions> options) : IKeycloakLoginService
{
    private readonly KeycloakOptions _options = options.Value;

    public Uri BuildLoginUri(string state)
    {
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = _options.ResponseType,
            ["scope"] = _options.Scope,
            ["state"] = state
        };

        return BuildUri(_options.Authority, "protocol/openid-connect/auth", query);
    }

    public async Task<LoginResponse> CompleteLoginAsync(string code, CancellationToken cancellationToken = default)
    {
        var tokenEndpoint = BuildUri(_options.EffectiveBackchannelAuthority, "protocol/openid-connect/token");
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _options.ClientId,
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri
        };

        if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            form["client_secret"] = _options.ClientSecret;
        }

        using var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new UnauthorizedAccessException($"Keycloak token exchange failed: {details}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new UnauthorizedAccessException("Keycloak token response was empty.");

        var userId = await GetUserIdAsync(tokenResponse, cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Keycloak response did not contain a user identifier.");
        }

        var accessToken = jwtTokenService.GenerateToken(userId);
        return new LoginResponse
        {
            UserId = userId,
            AccessToken = accessToken,
            ExpiresAtUtc = jwtTokenService.GetTokenExpiration(accessToken)
        };
    }

    public Uri BuildUiRedirectUri(LoginResponse response)
    {
        var query = new Dictionary<string, string?>
        {
            ["userId"] = response.UserId,
            ["accessToken"] = response.AccessToken,
            ["expiresAtUtc"] = response.ExpiresAtUtc.ToString("O")
        };

        return BuildUri(_options.UiRedirectUri, null, query);
    }

    private async Task<string?> GetUserIdAsync(
        KeycloakTokenResponse tokenResponse,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            using var userInfoRequest = new HttpRequestMessage(
                HttpMethod.Get,
                BuildUri(_options.EffectiveBackchannelAuthority, "protocol/openid-connect/userinfo"));
            userInfoRequest.Headers.Authorization = new("Bearer", tokenResponse.AccessToken);

            using var userInfoResponse = await httpClient.SendAsync(userInfoRequest, cancellationToken);
            if (userInfoResponse.IsSuccessStatusCode)
            {
                using var document = await JsonDocument.ParseAsync(
                    await userInfoResponse.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);

                if (TryGetString(document.RootElement, "sub", out var subject))
                {
                    return subject;
                }

                if (TryGetString(document.RootElement, "preferred_username", out var preferredUsername))
                {
                    return preferredUsername;
                }
            }
        }

        var token = !string.IsNullOrWhiteSpace(tokenResponse.IdToken)
            ? tokenResponse.IdToken
            : tokenResponse.AccessToken;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
               ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
               ?? jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        if (element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString()))
        {
            value = property.GetString()!;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static Uri BuildUri(string baseUri, string? path, IReadOnlyDictionary<string, string?>? query = null)
    {
        var uriBuilder = new UriBuilder(baseUri);

        if (!string.IsNullOrWhiteSpace(path))
        {
            uriBuilder.Path = $"{uriBuilder.Path.TrimEnd('/')}/{path.TrimStart('/')}";
        }

        if (query is not null && query.Count > 0)
        {
            uriBuilder.Query = string.Join("&", query
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}"));
        }

        return uriBuilder.Uri;
    }
}
