namespace Ube.Application.Features.Payments;

public interface IRefundService
{
    Task<RefundDto> RequestAsync(Guid requestedByUserId, bool isAdmin, RequestRefundRequest request, CancellationToken ct = default);
    Task<RefundDto> ApproveAsync(Guid approvedByUserId, Guid refundId, CancellationToken ct = default);
    Task<RefundDto> RejectAsync(Guid actorUserId, Guid refundId, string reason, CancellationToken ct = default);
}
