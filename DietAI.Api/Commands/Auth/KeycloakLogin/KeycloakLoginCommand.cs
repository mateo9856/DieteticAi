using MediatR;

namespace DietAI.Api.Commands.Auth.KeycloakLogin;

public class KeycloakLoginCommand : IRequest<KeycloakLoginResult>;
