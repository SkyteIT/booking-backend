namespace Ube.Application.Features.Search;

public class SearchListingsRequest
{
    public List<Guid> CategoryIds { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? Location { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public bool? IsAvailable { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
