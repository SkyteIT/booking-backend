using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IPayoutExportRepository
{
    Task<PayoutExportRun?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Any admin needs to be able to discover a run someone else started -
    // it was previously only visible to whoever's browser requested it.
    Task<IReadOnlyList<PayoutExportRun>> GetPendingAsync(CancellationToken ct = default);

    Task AddAsync(PayoutExportRun run, CancellationToken ct = default);
    Task UpdateAsync(PayoutExportRun run, CancellationToken ct = default);
}
