using FluentValidation;

namespace DietAI.Api.Commands.Auth.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}