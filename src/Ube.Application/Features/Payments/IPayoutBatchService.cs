namespace Ube.Application.Features.Payments;

public interface IPayoutBatchService
{
    Task<PayoutBatchDto> ComputeAsync(ComputePayoutBatchRequest request, CancellationToken ct = default);
    Task<PayoutBatchDto> SettleAsync(Guid settledByUserId, Guid batchId, CancellationToken ct = default);
    Task<IReadOnlyList<PayoutBatchDto>> GetForVendorAsync(Guid vendorProfileId, CancellationToken ct = default);
}
