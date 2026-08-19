namespace Ube.Application.DTOs.Listings;

public sealed class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? Type { get; set; }
}