using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Refund>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default);
    Task AddAsync(Refund refund, CancellationToken ct = default);
    Task UpdateAsync(Refund refund, CancellationToken ct = default);
}
