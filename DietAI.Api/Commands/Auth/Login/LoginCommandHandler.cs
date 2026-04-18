using DietAI.Api.Services.Login.Abstractions;
using DietAI.Api.Services.Login.Models;
using DietAI.Api.Services.Login.Requests;
using MediatR;

namespace DietAI.Api.Commands.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly ILoginService _loginService;

    public LoginCommandHandler(ILoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest
        {
            Username = request.Username,
            Password = request.Password
        };

        return await _loginService.LoginAsync(loginRequest, cancellationToken);
    }
}