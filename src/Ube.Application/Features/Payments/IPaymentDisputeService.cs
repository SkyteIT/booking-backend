using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public interface IPaymentDisputeService
{
    Task<PaymentDisputeDto> RecordDisputeAsync(Guid actorUserId, RecordDisputeRequest request, CancellationToken ct = default);
    Task<PaymentDisputeDto> ResolveDisputeAsync(Guid actorUserId, Guid disputeId, ResolveDisputeRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDisputeDto>> GetForPaymentAsync(Guid paymentId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDisputeDto>> GetForVendorAsync(Guid vendorProfileId, CancellationToken ct = default);
    Task<PagedResult<AdminDisputeDto>> GetPagedAsync(PaymentDisputeStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
}
