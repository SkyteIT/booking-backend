using Ube.Application.Features.Search;

namespace Ube.Tests.Listings;

public class SearchQueryTokenizerTests
{
    [Fact]
    public void Tokenize_Removes_StopWords_And_Punctuation()
    {
        var tokens = SearchQueryTokenizer.Tokenize("Rooftop dinner in Lisbon!");

        Assert.Equal(new[] { "rooftop", "dinner", "lisbon" }, tokens);
    }

    [Fact]
    public void Tokenize_Deduplicates_And_Normalizes_Case()
    {
        var tokens = SearchQueryTokenizer.Tokenize("Kandy hotel, kandy HOTEL");

        Assert.Equal(new[] { "kandy", "hotel" }, tokens);
    }
}
