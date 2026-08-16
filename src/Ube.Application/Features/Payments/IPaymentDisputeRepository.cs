using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IPaymentDisputeRepository
{
    Task<PaymentDispute?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDispute>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDispute>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default);
    Task AddAsync(PaymentDispute dispute, CancellationToken ct = default);
    Task UpdateAsync(PaymentDispute dispute, CancellationToken ct = default);
}
