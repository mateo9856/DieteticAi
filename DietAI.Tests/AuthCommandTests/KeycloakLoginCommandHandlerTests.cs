using DietAI.Api.Commands.Auth.KeycloakLogin;
using DietAI.Api.Services.Keycloak.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace DietAI.Tests.AuthCommandTests;

public class KeycloakLoginCommandHandlerTests
{
    [Test]
    public async Task Handle_GeneratesStateAndBuildsLoginUri()
    {
        var keycloakLoginService = Substitute.For<IKeycloakLoginService>();
        keycloakLoginService
            .BuildLoginUri(Arg.Any<string>())
            .Returns(call => new Uri($"https://identity.example.com/login?state={call.Arg<string>()}"));

        var handler = new KeycloakLoginCommandHandler(keycloakLoginService);

        var result = await handler.Handle(
            new KeycloakLoginCommand(),
            CancellationToken.None);

        result.State.Should().NotBeNullOrWhiteSpace();
        result.LoginUri.Should().Be(new Uri($"https://identity.example.com/login?state={result.State}"));
        keycloakLoginService.Received(1).BuildLoginUri(result.State);
    }
}
