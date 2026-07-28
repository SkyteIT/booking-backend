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
    Task<IReadOnlyList<LedgerEntry>> GetUnbatchedByVendorIdAsync(Guid vendorProfileId, DateTime periodEnd, CancellationToken ct = default);
}
