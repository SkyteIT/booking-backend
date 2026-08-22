namespace Ube.Application.Features.Listings;

public class RestaurantDetailsDto
{
    public string CuisineType { get; set; } = string.Empty;
    public decimal AverageCost { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
    // Edit-form friendly fields: vendors type the numeric time and select
    // AM/PM separately. OpeningHours remains for backwards compatibility.
    public string? OpeningTime { get; set; }
    public string? OpeningPeriod { get; set; }
    public string? ClosingTime { get; set; }
    public string? ClosingPeriod { get; set; }
    public int TableCapacity { get; set; }
    public List<string>? TableTypes { get; set; }
    public string? ReservationRules { get; set; }
}
