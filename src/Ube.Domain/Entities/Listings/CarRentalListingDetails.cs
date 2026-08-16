namespace Ube.Domain.Entities.Listings;

public class CarRentalListingDetails : IListingDetail
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public int SeatCount { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;
    public int? Year { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? PickupLocation { get; set; }
    public string? ReturnLocation { get; set; }
    public string? InsuranceOptions { get; set; }
}