namespace DietAI.Api.Commands.Auth.KeycloakLogin;

public class KeycloakLoginResult
{
    public required string State { get; init; }

    public required Uri LoginUri { get; init; }
}
