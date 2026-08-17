using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

// Joins a Refund with just enough context (from Payment -> Booking ->
// Customer/Listing/VendorProfile) to display in an admin queue - Refund
// itself has no navigation properties to those, by design.
public record RefundListItem(
    Refund Refund,
    string BookingNumber,
    string CustomerName,
    string VendorName,
    string ListingTitle);

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Refund>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<(List<RefundListItem> Items, int TotalCount)> GetPagedAsync(
        RefundStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AddAsync(Refund refund, CancellationToken ct = default);
    Task UpdateAsync(Refund refund, CancellationToken ct = default);
}
