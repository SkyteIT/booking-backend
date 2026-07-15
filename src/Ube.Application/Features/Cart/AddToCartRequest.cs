namespace Ube.Application.Features.Cart;

public class AddToCartRequest
{
    public Guid ListingId { get; set; }
    public int Quantity { get; set; }
}
