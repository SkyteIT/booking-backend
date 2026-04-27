namespace Ube.Application.DTOs.Cart;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ListingId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime AddedAt { get; set; }
}
