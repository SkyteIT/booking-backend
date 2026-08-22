using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingResponse
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SelectedCategoryId { get; set; }
    public ListingSelectedCategoryDto? SelectedCategory { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? Location { get; set; }
    public bool IsActive { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string CategoryDisplayName { get; set; } = string.Empty;
    public bool IsCategoryEditable { get; set; }
    public string VendorName { get; set; } = string.Empty;
    // What Listing.Price is denominated in (nightly/hourly/per-person/
    // daily/flat), from the listing's Category - lets the frontend cart
    // compute an accurate live total estimate matching BookingPricingRules.
    public PricingUnit? PricingUnit { get; set; }

    public ListingType Type { get; set; }
    public ListingType CategoryType { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? PrimaryImage { get; set; }

    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? CancellationPolicy { get; set; }

    public bool HasActiveOffer { get; set; }
    public string? OfferBadgeText { get; set; }

    public HotelDetailsDto? HotelDetails { get; set; }
    public RestaurantDetailsDto? RestaurantDetails { get; set; }
    public EventDetailsDto? EventDetails { get; set; }
    public CarRentalDetailsDto? CarRentalDetails { get; set; }
    public ActivityDetailsDto? ActivityDetails { get; set; }

    public List<ListingUnitDto> BookableUnits { get; set; } = new();
    public BookingSelectionConfigDto BookingSelection { get; set; } = new();

    public List<ListingCustomFieldValueDto> CustomFieldValues { get; set; } = new();
}

public class BookingSelectionConfigDto
{
    public string StartLabel { get; set; } = string.Empty;
    public string? EndLabel { get; set; }
    public bool ShowStartDate { get; set; }
    public bool ShowStartTime { get; set; }
    public bool ShowEndDate { get; set; }
    public bool ShowEndTime { get; set; }
    public bool EndMustBeAfterStart { get; set; }
    public string QuantityLabel { get; set; } = string.Empty;
    public string? UnitLabel { get; set; }
    public bool ShowUnitSelection { get; set; }
    public DateTime? FixedStartDateTime { get; set; }
}

public class ListingSelectedCategoryDto
{
    public Guid Id { get; set; }
    public Guid Value { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public ListingType Type { get; set; }
}
