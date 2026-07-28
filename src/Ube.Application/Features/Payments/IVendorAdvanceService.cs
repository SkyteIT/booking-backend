namespace Ube.Application.Features.Payments;

public interface IVendorAdvanceService
{
    Task<VendorAdvanceDto> IssueAdvanceAsync(Guid actorUserId, IssueVendorAdvanceRequest request, CancellationToken ct = default);
}
