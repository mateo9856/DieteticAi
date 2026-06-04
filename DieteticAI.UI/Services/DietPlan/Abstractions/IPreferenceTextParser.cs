namespace DieteticAI.UI.Services.DietPlan.Abstractions;

public interface IPreferenceTextParser
{
    List<string> Parse(string text, int maxItems, string fieldName);
}
