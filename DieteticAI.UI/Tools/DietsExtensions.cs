using DieteticAI.UI.Services.AiPlanSender.Enums;

namespace DieteticAI.UI.Tools;

public class DietsExtensions
{
    public decimal CalculateCalories(int age, decimal weight, SexEnum sex)
    {
        // Simple calorie calculation based on BMR
        decimal bmr;
        if (sex == SexEnum.Male)
        {
            bmr = 88.362m + (13.397m * weight) + (4.799m * age) - (5.677m * age);
        }
        else
        {
            bmr = 447.593m + (9.247m * weight) + (3.098m * age) - (4.330m * age);
        }
        
        // Add activity factor (1.2 for sedentary)
        return Math.Round(bmr * 1.2m, 0);
    }
}
