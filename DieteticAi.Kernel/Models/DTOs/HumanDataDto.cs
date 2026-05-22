using System.ComponentModel.DataAnnotations;
using DieteticAi.Models;

namespace DietAI.AiKernel.Models.DTOs;

public class HumanDataDto
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

    [Required]
    public GoalType GoalType { get; set; }

    [Required]
    public ActivityLevel ActivityLevel { get; set; }

    [Range(2, 6)]
    public int MealsPerDay { get; set; } = 3;

    public List<string> Allergies { get; set; } = [];

    public List<string> ExcludedIngredients { get; set; } = [];
}
