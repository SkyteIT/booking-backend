namespace Ube.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool RequiresAdminApproval { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ListingCount { get; set; }
}
