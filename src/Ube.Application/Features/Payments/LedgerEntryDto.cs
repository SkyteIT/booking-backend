using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

// Same shape as the LedgerEntry entity, plus the human-readable
// BookingNumber - the raw BookingId guid isn't something a vendor can
// recognize their own booking by.
public class LedgerEntryDto
{
    public Guid Id { get; set; }
    public LedgerAccountType AccountType { get; set; }
    public Guid? VendorProfileId { get; set; }
    public LedgerEntryType EntryType { get; set; }
    public LedgerDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? RefundId { get; set; }
    public Guid? PayoutBatchId { get; set; }
    public Guid? VendorCommissionInvoiceId { get; set; }
    public DateTime CreatedAt { get; set; }
}
