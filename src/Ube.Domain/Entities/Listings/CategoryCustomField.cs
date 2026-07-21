using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

// Admin-defined per-category field (e.g. "Amenities" as a Dropdown with
// options WiFi/Parking/Pool for the Hotel category). Options is a comma
// separated list, only meaningful when FieldType == Dropdown - matches how
// every other option list in this codebase (Tags, Amenities, RoomTypes...)
// is stored, rather than a child table for a handful of short strings.
public class CategoryCustomField
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Label { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
    public bool Required { get; set; }
    public int DisplayOrder { get; set; }
    public string? Options { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
