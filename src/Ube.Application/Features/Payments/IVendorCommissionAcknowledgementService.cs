namespace Ube.Application.Features.Payments;

public interface IVendorCommissionAcknowledgementService
{
    // Resolves the vendor's CURRENT rate for a category and records that
    // they were shown it - a timestamped, provable record, not a UI
    // checkbox that's easy to lose track of.
    Task<CommissionAcknowledgementDto> AcknowledgeAsync(Guid vendorProfileId, AcknowledgeCommissionRateRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CommissionAcknowledgementDto>> GetHistoryAsync(Guid vendorProfileId, CancellationToken ct = default);
}
