using Ube.Domain.Enums;

namespace Ube.Domain.Entities.Listings;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Configuration
    public string? BookingType { get; set; }
    public string? ServiceModel { get; set; }
    public bool DateSelectionEnabled { get; set; }
    public bool TimeSlotEnabled { get; set; }
    public bool AvailabilityCalendarEnabled { get; set; }

    // Pricing
    public decimal DefaultCommissionPercent { get; set; } = 15;
    public decimal? PlatformServiceFee { get; set; }
    public bool TaxApplicable { get; set; }

    // Display
    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }

    // Control
    public bool RequiresAdminApproval { get; set; }

    // 3-state status: Active, Inactive, Deleted (needed for soft-delete)
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
