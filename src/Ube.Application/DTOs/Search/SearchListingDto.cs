namespace Ube.Application.DTOs.Search;

public class SearchListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PriceFrom { get; set; }
    public decimal Rating { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsAvailable { get; set; }
    public string? ThumbnailUrl { get; set; }
}
