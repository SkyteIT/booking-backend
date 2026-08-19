namespace Ube.Application.DTOs.Cart;

public sealed class UpdateCartItemRequest
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
    public int? GuestCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}