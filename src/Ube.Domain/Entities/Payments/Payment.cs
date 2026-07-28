using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

public class Payment
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }
    public Guid VendorProfileId { get; set; }
    public Guid InitiatedByUserId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;

    public PaymentCollectionMethod CollectionMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? GatewayReference { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;

    // Snapshotted at charge time - never recalculated after the fact.
    public decimal CommissionPercentApplied { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal NetVendorAmount { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
