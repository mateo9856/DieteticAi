using System.ComponentModel.DataAnnotations;

namespace DietAI.Api.Options;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required]
    public string Authority { get; init; } = string.Empty;

    public string? BackchannelAuthority { get; init; }

    [Required]
    public string ClientId { get; init; } = string.Empty;

    public string? ClientSecret { get; init; }

    [Required]
    public string RedirectUri { get; init; } = string.Empty;

    [Required]
    public string UiRedirectUri { get; init; } = string.Empty;

    public string ResponseType { get; init; } = "code";

    public string Scope { get; init; } = "openid profile email";

    public string EffectiveBackchannelAuthority => string.IsNullOrWhiteSpace(BackchannelAuthority)
        ? Authority
        : BackchannelAuthority;
}
