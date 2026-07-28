using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

// Deliberately insert-only: no Update/Delete method exists on this
// interface. A correction is always a new compensating entry (Reversal /
// Clawback), never an edit of an existing row. Do not add an update method
// here - that would defeat the immutability guarantee the ledger exists
// to provide.
public interface ILedgerRepository
{
    Task AddAsync(LedgerEntry entry, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<LedgerEntry> entries, CancellationToken ct = default);
    Task<IReadOnlyList<LedgerEntry>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default);
    Task<IReadOnlyList<LedgerEntry>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default);

    // The running balance as of a point in time - ALL of a vendor's
    // entries up to asOf, unfiltered by PayoutBatchId/VendorCommissionInvoiceId.
    // Settlement/InvoicePayment entries must be included here, not
    // excluded, since they're what nets out what's already been resolved -
    // excluding them would double-count old entries on every recomputation.
    Task<IReadOnlyList<LedgerEntry>> GetByVendorIdAsOfAsync(Guid vendorProfileId, DateTime asOf, CancellationToken ct = default);
}
