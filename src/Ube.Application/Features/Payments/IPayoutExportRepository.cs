using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IPayoutExportRepository
{
    Task<PayoutExportRun?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PayoutExportRun run, CancellationToken ct = default);
    Task UpdateAsync(PayoutExportRun run, CancellationToken ct = default);
}
