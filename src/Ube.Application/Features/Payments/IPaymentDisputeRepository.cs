using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public record DisputeListItem(
    PaymentDispute Dispute,
    string BookingNumber,
    string CustomerName,
    string VendorName);

public interface IPaymentDisputeRepository
{
    Task<PaymentDispute?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDispute>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDispute>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default);
    Task<(List<DisputeListItem> Items, int TotalCount)> GetPagedAsync(
        PaymentDisputeStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AddAsync(PaymentDispute dispute, CancellationToken ct = default);
    Task UpdateAsync(PaymentDispute dispute, CancellationToken ct = default);
}
