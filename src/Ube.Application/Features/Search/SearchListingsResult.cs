namespace Ube.Application.Features.Search;

public class SearchListingsResult
{
    public IReadOnlyList<SearchListingDto> Items { get; set; } = new List<SearchListingDto>();
    public int TotalCount { get; set; }
}
