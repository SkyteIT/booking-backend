using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IPayoutBatchRepository
{
    Task<PayoutBatch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PayoutBatch>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default);
    Task AddAsync(PayoutBatch batch, CancellationToken ct = default);
    Task UpdateAsync(PayoutBatch batch, CancellationToken ct = default);
}
