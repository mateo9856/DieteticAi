namespace DieteicAi.Models;

public class Diets
{
    public int Id { get; set; }
    
    public string DietName { get; set; }
    
    public string Description { get; set; }
    
    public int Age { get; set; }
    
    public decimal ForWeight { get; set; }
    
    public decimal CaloricValue { get; set; }
    
    public SexEnum ForSex { get; set; }
}