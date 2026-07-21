namespace Ube.Application.Features.Listings;

public class UpdateListingRequest
{
    // ListingType is derived from the chosen Category.Type - the vendor
    // doesn't pick it separately, so there's no Type field here.
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? CancellationPolicy { get; set; }

    public HotelDetailsDto? HotelDetails { get; set; }
    public RestaurantDetailsDto? RestaurantDetails { get; set; }
    public EventDetailsDto? EventDetails { get; set; }
    public CarRentalDetailsDto? CarRentalDetails { get; set; }
    public ActivityDetailsDto? ActivityDetails { get; set; }

    // Answers to the chosen category's admin-defined custom fields
    // (e.g. Amenities, RoomTypes, TableTypes).
    public List<ListingCustomFieldValueInputDto> CustomFieldValues { get; set; } = new();
}
