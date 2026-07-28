namespace Ube.Application.Features.Payments;

// The external gateway is the ultimate source of truth for whether money
// actually moved - our own database is only as trustworthy as its
// agreement with it. This boundary exists now, even against the mock
// gateway (which will always "agree" with itself), so the seam is already
// in place before a real gateway - and its own failure modes - exists.
public interface IPaymentReconciliationService
{
    Task<ReconciliationReportDto> ReconcileAsync(Guid actorUserId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
}
