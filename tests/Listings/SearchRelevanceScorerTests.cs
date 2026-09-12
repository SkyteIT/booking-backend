using Ube.Application.Features.Search;

namespace Ube.Tests.Listings;

public class SearchRelevanceScorerTests
{
    [Fact]
    public void Score_Prefers_Title_And_Category_Prefix_Matches()
    {
        var query = "colombo rest";

        var strongMatch = new SearchListingScoringInput(
            Title: "Colombo Restaurant & Dining",
            CategoryName: "Restaurant & Dining",
            Location: "Colombo",
            Description: null,
            OriginalCategoryName: null,
            Tags: null,
            VendorBusinessName: null,
            VendorBusinessType: null);

        var weakMatch = new SearchListingScoringInput(
            Title: "Dining in Colombo",
            CategoryName: "Food",
            Location: "Colombo",
            Description: null,
            OriginalCategoryName: null,
            Tags: null,
            VendorBusinessName: null,
            VendorBusinessType: null);

        var strongScore = SearchRelevanceScorer.Score(query, strongMatch);
        var weakScore = SearchRelevanceScorer.Score(query, weakMatch);

        Assert.True(strongScore > weakScore);
    }

    [Fact]
    public void Score_Gives_Prefix_Matches_More_Weight_Than_Generic_Contains()
    {
        var query = "rest";

        var prefixMatch = new SearchListingScoringInput(
            Title: "Restaurant & Dining",
            CategoryName: "Restaurant & Dining",
            Location: "Galle",
            Description: null,
            OriginalCategoryName: null,
            Tags: null,
            VendorBusinessName: null,
            VendorBusinessType: null);

        var containsMatch = new SearchListingScoringInput(
            Title: "Best places to eat",
            CategoryName: "Dining and Restaurants",
            Location: "Galle",
            Description: null,
            OriginalCategoryName: null,
            Tags: null,
            VendorBusinessName: null,
            VendorBusinessType: null);

        var prefixScore = SearchRelevanceScorer.Score(query, prefixMatch);
        var containsScore = SearchRelevanceScorer.Score(query, containsMatch);

        Assert.True(prefixScore > containsScore);
    }

    [Fact]
    public void IsMatch_Allows_Common_Typo_Variants()
    {
        var listing = new SearchListingScoringInput(
            Title: "Restaurant & Dining",
            CategoryName: "Restaurant & Dining",
            Location: "Colombo",
            Description: null,
            OriginalCategoryName: null,
            Tags: null,
            VendorBusinessName: null,
            VendorBusinessType: null);

        Assert.True(SearchRelevanceScorer.IsMatch("restuarant", listing));
    }

    [Fact]
    public void IsMatch_Requires_All_Query_Tokens_To_Match_Some_Field()
    {
        var listing = new SearchListingScoringInput(
            Title: "Beach View Dining",
            CategoryName: "Restaurant & Dining",
            Location: "Colombo",
            Description: null,
            OriginalCategoryName: null,
            Tags: null,
            VendorBusinessName: null,
            VendorBusinessType: null);

        Assert.True(SearchRelevanceScorer.IsMatch("colombo rest", listing));
        Assert.False(SearchRelevanceScorer.IsMatch("colombo spa", listing));
    }
}
