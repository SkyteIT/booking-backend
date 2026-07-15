namespace Ube.Application.Features.Listings;

public class RestaurantDetailsDto
{
    public string CuisineType { get; set; } = string.Empty;
    public decimal AverageCost { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
    public List<string>? TableTypes { get; set; }
    public string? ReservationRules { get; set; }
}
