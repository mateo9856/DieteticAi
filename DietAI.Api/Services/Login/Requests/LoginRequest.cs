using System.ComponentModel.DataAnnotations;

namespace DietAI.Api.Services.Login.Requests;

public sealed class LoginRequest
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Password { get; set; } = string.Empty;
}
