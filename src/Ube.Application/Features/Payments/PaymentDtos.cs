using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class InitiatePaymentRequest
{
    public Guid BookingId { get; set; }
    public PaymentCollectionMethod CollectionMethod { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid VendorProfileId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentCollectionMethod CollectionMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayReference { get; set; }
    public decimal CommissionPercentApplied { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal NetVendorAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RequestRefundRequest
{
    public Guid PaymentId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RefundDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RefundStatus Status { get; set; }
    public decimal PolicyTierApplied { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Enriched view for the admin refund queue - adds the context
// (booking/customer/vendor/listing) a plain RefundDto doesn't have.
public class AdminRefundDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string ListingTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RefundStatus Status { get; set; }
    public decimal PolicyTierApplied { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
