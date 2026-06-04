using DieteticAI.UI.Services.DietPlan.Abstractions;

namespace DieteticAI.UI.Services.DietPlan.Implementations;

public sealed class PreferenceTextParser : IPreferenceTextParser
{
    public List<string> Parse(string text, int maxItems, string fieldName)
    {
        var values = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (values.Count > maxItems)
        {
            throw new InvalidOperationException($"{fieldName} cannot contain more than {maxItems} items.");
        }

        return values;
    }
}
