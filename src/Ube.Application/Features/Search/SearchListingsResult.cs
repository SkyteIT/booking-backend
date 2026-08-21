namespace Ube.Application.Features.Search;

public class SearchListingsResult
{
    public List<SearchListingDto> Items { get; set; } = new();

    public int TotalCount { get; set; }
}
