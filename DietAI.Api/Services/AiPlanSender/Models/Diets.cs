using DietAI.Api.Services.AiPlanSender.Enums;

namespace DietAI.Api.Services.AiPlanSender.Models;

public class Diets
{
    public int Id { get; set; }
    public string DietName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal ForWeight { get; set; }
    public decimal ForHeight { get; set; }
    public decimal CaloricValue { get; set; }
    public SexEnum ForSex { get; set; }
    public DietType DietType { get; set; }
}
