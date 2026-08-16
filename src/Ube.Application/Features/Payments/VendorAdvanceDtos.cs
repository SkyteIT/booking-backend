namespace Ube.Application.Features.Payments;

public class IssueVendorAdvanceRequest
{
    public Guid PaymentId { get; set; }
}

public class VendorAdvanceDto
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public Guid VendorProfileId { get; set; }
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; }
}
