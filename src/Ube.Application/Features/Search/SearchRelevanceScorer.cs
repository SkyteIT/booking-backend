namespace Ube.Application.Features.Search;

public sealed record SearchListingScoringInput(
    string Title,
    string CategoryName,
    string Location,
    string? Description,
    string? OriginalCategoryName,
    string? Tags,
    string? VendorBusinessName,
    string? VendorBusinessType);

public static class SearchRelevanceScorer
{
    public static int Score(string? searchTerm, SearchListingScoringInput listing)
    {
        return Score(
            SearchQueryTokenizer.Normalize(searchTerm),
            SearchQueryTokenizer.Tokenize(searchTerm),
            listing);
    }

    public static int Score(
        string normalizedQuery,
        IReadOnlyList<string> tokens,
        SearchListingScoringInput listing)
    {
        if (string.IsNullOrWhiteSpace(normalizedQuery) || tokens.Count == 0)
            return 0;

        var score = 0;
        var fuzzyTitle = SearchQueryTokenizer.NormalizeForFuzzy(listing.Title);
        var fuzzyCategory = SearchQueryTokenizer.NormalizeForFuzzy(listing.CategoryName);
        var fuzzyLocation = SearchQueryTokenizer.NormalizeForFuzzy(listing.Location);
        var fuzzyOriginalCategory = SearchQueryTokenizer.NormalizeForFuzzy(listing.OriginalCategoryName);
        var fuzzyVendorName = SearchQueryTokenizer.NormalizeForFuzzy(listing.VendorBusinessName);
        var fuzzyVendorType = SearchQueryTokenizer.NormalizeForFuzzy(listing.VendorBusinessType);
        var fuzzyTags = SearchQueryTokenizer.NormalizeForFuzzy(listing.Tags);
        var fuzzyDescription = SearchQueryTokenizer.NormalizeForFuzzy(listing.Description);

        score += PhraseScore(normalizedQuery, listing.Title, 120);
        score += PhraseScore(normalizedQuery, listing.CategoryName, 110);
        score += PhraseScore(normalizedQuery, listing.Location, 100);
        score += PhraseScore(normalizedQuery, listing.OriginalCategoryName, 90);
        score += PhraseScore(normalizedQuery, listing.VendorBusinessName, 85);
        score += PhraseScore(normalizedQuery, listing.VendorBusinessType, 80);
        score += PhraseScore(normalizedQuery, listing.Tags, 60);
        score += PhraseScore(normalizedQuery, listing.Description, 35);

        foreach (var token in tokens)
        {
            score += TokenScore(token, listing.Title, 40, 24);
            score += TokenScore(token, listing.CategoryName, 38, 22);
            score += TokenScore(token, listing.Location, 36, 20);
            score += TokenScore(token, listing.OriginalCategoryName, 34, 18);
            score += TokenScore(token, listing.VendorBusinessName, 32, 16);
            score += TokenScore(token, listing.VendorBusinessType, 30, 15);
            score += TokenScore(token, listing.Tags, 22, 10);
            score += TokenScore(token, listing.Description, 14, 6);

            var fuzzyToken = SearchQueryTokenizer.NormalizeForFuzzy(token);
            score += FuzzyTokenScore(fuzzyToken, fuzzyTitle, 18);
            score += FuzzyTokenScore(fuzzyToken, fuzzyCategory, 16);
            score += FuzzyTokenScore(fuzzyToken, fuzzyLocation, 15);
            score += FuzzyTokenScore(fuzzyToken, fuzzyOriginalCategory, 14);
            score += FuzzyTokenScore(fuzzyToken, fuzzyVendorName, 12);
            score += FuzzyTokenScore(fuzzyToken, fuzzyVendorType, 11);
            score += FuzzyTokenScore(fuzzyToken, fuzzyTags, 8);
            score += FuzzyTokenScore(fuzzyToken, fuzzyDescription, 5);
        }

        return score;
    }

    public static bool IsMatch(string? searchTerm, SearchListingScoringInput listing)
    {
        var tokens = SearchQueryTokenizer.Tokenize(searchTerm);
        if (tokens.Count == 0)
            return true;

        return tokens.All(token => MatchesToken(token, listing));
    }

    private static int PhraseScore(string normalizedQuery, string? field, int weight)
    {
        if (string.IsNullOrWhiteSpace(field))
            return 0;

        return NormalizeField(field).Contains(normalizedQuery, StringComparison.Ordinal)
            ? weight
            : 0;
    }

    private static int TokenScore(string token, string? field, int prefixWeight, int containsWeight)
    {
        if (string.IsNullOrWhiteSpace(field))
            return 0;

        var normalizedField = NormalizeField(field);
        if (normalizedField.StartsWith(token, StringComparison.Ordinal))
            return prefixWeight;

        return normalizedField.Contains(token, StringComparison.Ordinal)
            ? containsWeight
            : 0;
    }

    private static int FuzzyTokenScore(string fuzzyToken, string? fuzzyField, int weight)
    {
        if (string.IsNullOrWhiteSpace(fuzzyToken) || string.IsNullOrWhiteSpace(fuzzyField))
            return 0;

        if (fuzzyField.StartsWith(fuzzyToken, StringComparison.Ordinal))
            return weight;

        return fuzzyField.Contains(fuzzyToken, StringComparison.Ordinal)
            ? weight
            : 0;
    }

    private static string NormalizeField(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return SearchQueryTokenizer.Normalize(value);
    }

    private static bool MatchesToken(string token, SearchListingScoringInput listing)
    {
        var fuzzyToken = SearchQueryTokenizer.NormalizeForFuzzy(token);
        var fields = new[]
        {
            listing.Title,
            listing.CategoryName,
            listing.Location,
            listing.OriginalCategoryName,
            listing.VendorBusinessName,
            listing.VendorBusinessType,
            listing.Tags,
            listing.Description
        };

        return fields.Any(field =>
        {
            var normalizedField = NormalizeField(field);
            var fuzzyField = SearchQueryTokenizer.NormalizeForFuzzy(field);

            return normalizedField.Contains(token, StringComparison.Ordinal)
                || normalizedField.StartsWith(token, StringComparison.Ordinal)
                || fuzzyField.Contains(fuzzyToken, StringComparison.Ordinal)
                || fuzzyField.StartsWith(fuzzyToken, StringComparison.Ordinal);
        });
    }
}
