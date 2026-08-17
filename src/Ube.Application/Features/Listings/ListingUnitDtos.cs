using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingUnitDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public ListingUnitKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public decimal? PriceOverride { get; set; }
    public int Capacity { get; set; }
    public int? RowIndex { get; set; }
    public int? ColumnIndex { get; set; }
    public TimeSpan? SlotStartTime { get; set; }
    public TimeSpan? SlotDuration { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class AddListingUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal? PriceOverride { get; set; }
    public int Capacity { get; set; } = 1;
    public int DisplayOrder { get; set; }
}

public class AddListingUnitsGridRequest
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public List<string>? RowLabels { get; set; }
    public decimal? PricePerSeat { get; set; }
}

public class AddListingUnitsTimeSlotsRequest
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public int CapacityPerSlot { get; set; } = 1;
    public decimal? Price { get; set; }
}

public class UpdateListingUnitRequest
{
    public string? Name { get; set; }
    public decimal? PriceOverride { get; set; }
    public int? Capacity { get; set; }
    public bool? IsActive { get; set; }
    public int? DisplayOrder { get; set; }
}
