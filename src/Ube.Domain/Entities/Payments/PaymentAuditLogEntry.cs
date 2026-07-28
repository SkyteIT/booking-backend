namespace Ube.Domain.Entities.Payments;

// Records decision points the money-movement ledger doesn't capture on its
// own: rejected refunds, webhook signature failures, payout batch reviews,
// commission override changes. Actor + reason, always.
public class PaymentAuditLogEntry
{
    public Guid Id { get; set; }

    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
