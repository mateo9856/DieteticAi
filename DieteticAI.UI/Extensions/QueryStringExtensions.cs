namespace DieteticAI.UI.Extensions;

public static class QueryStringExtensions
{
    public static IReadOnlyDictionary<string, string> ParseQueryString(this Uri uri)
    {
        return uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(
                parts => Uri.UnescapeDataString(parts[0]),
                parts => Uri.UnescapeDataString(parts[1].Replace("+", " ")),
                StringComparer.OrdinalIgnoreCase);
    }
}
