namespace Ube.Application.DTOs.Category;

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? RequiresAdminApproval { get; set; }

    /// <summary>
    /// Accepted values: "Active", "Inactive", "Deleted"
    /// </summary>
    public string? Status { get; set; }

    // Extended fields from the AddCategory form
    public string? BookingType { get; set; }
    public string? ServiceModel { get; set; }
    public bool? DateSelectionEnabled { get; set; }
    public bool? TimeSlotEnabled { get; set; }
    public bool? AvailabilityCalendarEnabled { get; set; }
    public decimal? DefaultCommissionPercent { get; set; }
    public decimal? PlatformServiceFee { get; set; }
    public bool? TaxApplicable { get; set; }
}
