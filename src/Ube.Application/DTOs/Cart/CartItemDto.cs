namespace Ube.Application.DTOs.Cart;

public sealed class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public int Quantity { get; set; }
    public int GuestCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}