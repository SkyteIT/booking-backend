namespace Ube.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }

    // Basic
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Configuration
    public string? BookingType { get; set; }
    public string? ServiceModel { get; set; }
    public bool DateSelectionEnabled { get; set; }
    public bool TimeSlotEnabled { get; set; }
    public bool AvailabilityCalendarEnabled { get; set; }

    // Pricing
    public decimal DefaultCommissionPercent { get; set; }
    public decimal? PlatformServiceFee { get; set; }
    public bool TaxApplicable { get; set; }

    // Media & display
    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }

    // Control
    public bool RequiresAdminApproval { get; set; }

    // Status & counts
    public string Status { get; set; } = string.Empty;
    public int ListingCount { get; set; }

    // Audit
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
