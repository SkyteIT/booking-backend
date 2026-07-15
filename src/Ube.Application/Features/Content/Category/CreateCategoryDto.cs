namespace Ube.Application.Features.Content.Category;

public class CreateCategoryDto
{
    // ── Basic Information ──
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // ── Category Configuration ──
    public string? BookingType { get; set; }
    public string? ServiceModel { get; set; }
    public bool DateSelectionEnabled { get; set; }
    public bool TimeSlotEnabled { get; set; }
    public bool AvailabilityCalendarEnabled { get; set; }

    // ── Pricing & Commission ──
    public decimal DefaultCommissionPercent { get; set; } = 15;
    public decimal? PlatformServiceFee { get; set; }
    public bool TaxApplicable { get; set; }

    // ── Media & Display ──
    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsFeatured { get; set; }

    // ── Listing Control ──
    public bool RequiresAdminApproval { get; set; }

    // ── Status ──
    /// <summary>Accepted values: "Active", "Inactive", "Deleted". Defaults to Active.</summary>
    public string Status { get; set; } = "Active";
}
