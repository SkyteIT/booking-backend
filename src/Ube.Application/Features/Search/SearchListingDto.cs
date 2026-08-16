namespace Ube.Application.Features.Search;

public class SearchListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double AverageRating { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public string? ThumbnailUrl { get; set; }
}
