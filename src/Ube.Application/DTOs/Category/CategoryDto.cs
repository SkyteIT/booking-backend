namespace Ube.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? BookingType { get; set; }
    public string? ServiceModel { get; set; }
    public bool DateSelectionEnabled { get; set; }
    public bool TimeSlotEnabled { get; set; }
    public bool AvailabilityCalendarEnabled { get; set; }

    public decimal DefaultCommissionPercent { get; set; }
    public decimal? PlatformServiceFee { get; set; }
    public bool TaxApplicable { get; set; }

    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }

    public bool RequiresAdminApproval { get; set; }

    public string Status { get; set; } = string.Empty;
    public int ListingCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
