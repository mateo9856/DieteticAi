using DietAI.Api.Services.Login.Models;
using MediatR;

namespace DietAI.Api.Commands.Auth.Login;

public class LoginCommand : IRequest<LoginResponse>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}