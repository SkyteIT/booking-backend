using Ube.Domain.Enums;
using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Which ListingType this category's listings must use. Null only for the
    // internal "Uncategorized" sentinel category, which can hold listings of
    // any type; every admin-created category must have a Type.
    public ListingType? Type { get; set; }

    // Configuration
    public BookingConfirmationType? BookingType { get; set; }
    // What Listing.Price is denominated in (nightly/hourly/per-person/
    // daily/flat) - drives BookingPricingRules.CalculateTotal. Field
    // name kept as ServiceModel to match the existing admin form/DTO
    // wire contract (was free text before this feature).
    public PricingUnit? ServiceModel { get; set; }
    // Whether the customer pays the platform up front or pays the
    // vendor directly at the venue - drives CheckoutService's choice of
    // PaymentCollectionMethod. New field (no admin UI equivalent
    // existed before this feature).
    public ServiceCollectionModel? PaymentCollectionModel { get; set; }
    public bool DateSelectionEnabled { get; set; }
    public bool TimeSlotEnabled { get; set; }
    public bool AvailabilityCalendarEnabled { get; set; }

    // Pricing
    public decimal DefaultCommissionPercent { get; set; } = 15;
    public decimal? PlatformServiceFee { get; set; }
    public bool TaxApplicable { get; set; }

    // Refund policy (SRS 7.4) - time-based tiers relative to booking start
    public int FullRefundDaysBefore { get; set; }
    public int PartialRefundDaysBefore { get; set; }
    public decimal PartialRefundPercent { get; set; }
    public decimal? RefundAutoApprovalThreshold { get; set; }

    // Vendor advances
    public bool AllowsVendorAdvance { get; set; }
    public decimal? AdvancePercent { get; set; }

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
    public ICollection<CategoryCustomField> CustomFields { get; set; } = new List<CategoryCustomField>();
}
