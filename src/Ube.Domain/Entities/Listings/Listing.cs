using Ube.Domain.Entities.Common;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;

namespace Ube.Domain.Entities.Listings;

public class Listing : BaseEntity
{
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Stores the name of the category this listing belongs to.
    /// When a category is deleted, listings are parked in __Uncategorized__
    /// but this field retains the original category name so they can be
    /// automatically re-linked when a category with that name is recreated.
    /// </summary>
    public string? OriginalCategoryName { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PriceFrom { get; set; }
    public decimal Rating { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsAvailable { get; set; }
    public string? ThumbnailUrl { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public Category Category { get; set; } = null!;
}