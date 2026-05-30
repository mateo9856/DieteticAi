using DietAI.Api.Services.Keycloak.Abstractions;
using MediatR;

namespace DietAI.Api.Commands.Auth.KeycloakLogin;

public class KeycloakLoginCommandHandler : IRequestHandler<KeycloakLoginCommand, KeycloakLoginResult>
{
    private readonly IKeycloakLoginService _keycloakLoginService;

    public KeycloakLoginCommandHandler(IKeycloakLoginService keycloakLoginService)
    {
        _keycloakLoginService = keycloakLoginService;
    }

    public Task<KeycloakLoginResult> Handle(KeycloakLoginCommand request, CancellationToken cancellationToken)
    {
        var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        return Task.FromResult(new KeycloakLoginResult
        {
            State = state,
            LoginUri = _keycloakLoginService.BuildLoginUri(state)
        });
    }
}
