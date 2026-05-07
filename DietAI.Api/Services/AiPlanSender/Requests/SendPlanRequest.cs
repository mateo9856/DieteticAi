using System.ComponentModel.DataAnnotations;
using DietAI.Api.Services.AiPlanSender.Enums;

namespace DietAI.Api.Services.AiPlanSender.Requests;

public class SendPlanRequest
{
    [Required]
    [Range(15, 100)]
    public int Age { get; set; }

    [Required]
    [Range(10.00, 1500.00)]
    public decimal ActualWeight { get; set; }

    [Required]
    [Range(10.00, 300.00)]
    public decimal ActualHeight { get; set; }

    [Required]
    public SexEnum Sex { get; set; }

    [Required]
    public DietType DietType { get; set; }

    public decimal CaloricDemand { get; set; }
}
