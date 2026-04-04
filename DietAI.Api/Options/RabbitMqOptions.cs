using System.ComponentModel.DataAnnotations;

namespace DietAI.Api.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "rabbitMq";

    [Required]
    public string HostName { get; init; } = "localhost";

    [Required]
    public string UserName { get; init; } = "guest";

    [Required]
    public string Password { get; init; } = "guest";

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    public string CertPath { get; init; } = string.Empty;

    [Required]
    public string VirtualHost { get; init; } = "/";
}
