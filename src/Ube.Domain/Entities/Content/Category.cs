using Ube.Domain.Entities.Common;
using Ube.Domain.Enums;
using Ube.Domain.Entities.Listings;

namespace Ube.Domain.Entities.Content;

public class Category : BaseEntity
{
    // ── Basic ──
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // ── Configuration ──
    public string? BookingType { get; set; }
    public string? ServiceModel { get; set; }
    public bool DateSelectionEnabled { get; set; }
    public bool TimeSlotEnabled { get; set; }
    public bool AvailabilityCalendarEnabled { get; set; }

    // ── Pricing ──
    public decimal DefaultCommissionPercent { get; set; } = 15;
    public decimal? PlatformServiceFee { get; set; }
    public bool TaxApplicable { get; set; }

    // ── Media & Display ──
    public string? Icon { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }

    // ── Control ──
    public bool RequiresAdminApproval { get; set; }

    // ── Status ──
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    // ── Navigation ──
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
