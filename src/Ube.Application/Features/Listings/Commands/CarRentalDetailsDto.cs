namespace Ube.Application.Features.Listings.Commands;

public class CarRentalDetailsDto
{
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
}