namespace DieteticAi.Models;

public class Diets
{
    public int Id { get; set; }
    
    public string DietName { get; set; }
    
    public string Description { get; set; }
    
    public int Age { get; set; }
    
    public decimal ForWeight { get; set; }
    
    public decimal ForHeight { get; set; }
    
    public decimal CaloricValue { get; set; }
    
    public SexEnum ForSex { get; set; }
    
    public DietType DietType { get; set; }

    public GoalType GoalType { get; set; }

    public ActivityLevel ActivityLevel { get; set; }

    public int MealsPerDay { get; set; } = 3;

    public List<string> Allergies { get; set; } = [];

    public List<string> ExcludedIngredients { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }
}
