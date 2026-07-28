using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

// Insert-only. No UpdatedAt, no setter-based corrections - a mistake is
// fixed by writing a new compensating entry (Reversal/Clawback), never by
// editing this row. Enforced by ILedgerRepository exposing no update path.
public class LedgerEntry
{
    public Guid Id { get; set; }

    public LedgerAccountType AccountType { get; set; }
    public Guid? VendorProfileId { get; set; }

    public LedgerEntryType EntryType { get; set; }
    public LedgerDirection Direction { get; set; }
    public decimal Amount { get; set; }

    public Guid? BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? RefundId { get; set; }
    public Guid? PayoutBatchId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
