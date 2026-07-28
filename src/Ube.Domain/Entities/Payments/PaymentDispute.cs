using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

// A dispute is a different event than a Refund: it's the CUSTOMER'S BANK
// pulling money back from the platform, not the platform choosing to give
// money back. Recorded manually today (no real gateway/webhook exists yet
// to detect this automatically) - see RecordDisputeAsync.
public class PaymentDispute
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal? DisputeFeeAmount { get; set; }
    public string? ExternalDisputeReference { get; set; }

    public PaymentDisputeStatus Status { get; set; } = PaymentDisputeStatus.Opened;

    public Guid RecordedByUserId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
