using DietAI.Api.Services.Login.Models;

namespace DietAI.Api.Services.Keycloak.Abstractions;

public interface IKeycloakLoginService
{
    Uri BuildLoginUri(string state);

    Task<LoginResponse> CompleteLoginAsync(string code, CancellationToken cancellationToken = default);

    Uri BuildUiRedirectUri(LoginResponse response);
}
