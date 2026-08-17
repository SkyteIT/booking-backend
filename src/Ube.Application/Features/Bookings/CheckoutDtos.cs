namespace Ube.Application.Features.Bookings;

public class CheckoutItemRequest
{
    public Guid ListingId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public Guid? ListingUnitId { get; set; }
}

public class CheckoutRequest
{
    public List<CheckoutItemRequest> Items { get; set; } = new();
    public string IdempotencyKey { get; set; } = string.Empty;
    // No CollectionMethod here - it's derived per item from
    // Category.PaymentCollectionModel, never a customer choice.
}

public class CheckoutResultDto
{
    public List<BookingDetailDto> Bookings { get; set; } = new();
    public List<Payments.PaymentDto> Payments { get; set; } = new();
}
