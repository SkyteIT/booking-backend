namespace Ube.Application.DTOs.Cart;

public sealed class AddToCartRequest
{
    public Guid ListingId { get; set; }
    public int Quantity { get; set; } = 1;
    public int GuestCount { get; set; } = 1;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}