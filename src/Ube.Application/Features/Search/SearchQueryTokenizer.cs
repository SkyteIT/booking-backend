using System.Text.RegularExpressions;

namespace Ube.Application.Features.Search;

public static class SearchQueryTokenizer
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "at", "for", "in", "near", "of", "or", "the", "to", "with"
    };

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^\p{L}\p{Nd}]+", " ");
        return Regex.Replace(normalized, @"\s+", " ").Trim();
    }

    public static IReadOnlyList<string> Tokenize(string? value)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return Array.Empty<string>();

        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 2 && !StopWords.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string NormalizeForFuzzy(string? value)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return string.Empty;

        var withoutVowels = new string(normalized.Where(ch => !"aeiou".Contains(ch)).ToArray());
        return Regex.Replace(withoutVowels, @"\s+", " ").Trim();
    }
}
