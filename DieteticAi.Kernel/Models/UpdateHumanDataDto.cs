using System.ComponentModel.DataAnnotations;

namespace DieteticAi.Models;

public class UpdateHumanDataDto : HumanDataDto
{
    [Required]
    [Range(10.00, 1500.00)]
    public decimal PreviousWeight { get; set; }
    
    [Required]
    [Range(10.00, 300.00)]
    public decimal PreviousHeight { get; set; }
    
    [Required]
    public decimal PreviousCaloricDemand { get; set; }
}