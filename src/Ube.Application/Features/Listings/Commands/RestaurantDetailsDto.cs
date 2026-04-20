namespace Ube.Application.Features.Listings.Commands;

public class RestaurantDetailsDto
{
    public string CuisineType { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
    public decimal AverageCost { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
}