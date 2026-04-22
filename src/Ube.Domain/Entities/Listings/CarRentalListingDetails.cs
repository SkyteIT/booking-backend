namespace Ube.Domain.Entities.Listings;

public class CarRentalListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public int SeatCount { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;

    public Listing Listing { get; set; } = default!;
}