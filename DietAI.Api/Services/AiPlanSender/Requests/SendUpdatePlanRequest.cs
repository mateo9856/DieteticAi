using System.ComponentModel.DataAnnotations;

namespace DietAI.Api.Services.AiPlanSender.Requests;

public class SendUpdatePlanRequest : SendPlanRequest
{
    [Required]
    [Range(15, 100)]
    public int PreviousAge { get; set; }

    [Required]
    [Range(10.00, 1500.00)]
    public decimal PreviousWeight { get; set; }

    [Required]
    [Range(10.00, 300.00)]
    public decimal PreviousHeight { get; set; }

    public decimal PreviousCaloricDemand { get; set; }
}
