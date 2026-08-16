using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

// Covers the direction PayoutBatch doesn't: money the VENDOR owes the
// PLATFORM, built up from VendorCollected bookings (cash/card taken by the
// vendor directly) where our ledger already knows the commission is owed
// but no online payout exists to net it against automatically.
public class VendorCommissionInvoice
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal AmountOwed { get; set; }
    public DateTime DueDate { get; set; }

    public VendorInvoiceStatus Status { get; set; } = VendorInvoiceStatus.Pending;

    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
