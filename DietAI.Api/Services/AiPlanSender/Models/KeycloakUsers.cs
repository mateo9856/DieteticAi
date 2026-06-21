using System.ComponentModel.DataAnnotations;
namespace DietAI.Api.Services.AiPlanSender.Models;

public class KeycloakUsers
{
    [Key]
    [Required]
    public string UniqueId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}