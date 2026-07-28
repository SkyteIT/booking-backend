using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IVendorCommissionRepository
{
    Task<VendorCommissionOverride?> GetActiveOverrideAsync(Guid vendorProfileId, Guid? categoryId, DateTime asOf, CancellationToken ct = default);
    Task<IReadOnlyList<VendorCommissionOverride>> GetAllForVendorAsync(Guid vendorProfileId, CancellationToken ct = default);
    Task AddOverrideAsync(VendorCommissionOverride commissionOverride, CancellationToken ct = default);
    Task UpdateOverrideAsync(VendorCommissionOverride commissionOverride, CancellationToken ct = default);

    Task<IReadOnlyList<LoyaltyDiscountTier>> GetLoyaltyTiersAsync(CancellationToken ct = default);

    Task AddAcknowledgementAsync(VendorCommissionAcknowledgement acknowledgement, CancellationToken ct = default);
    Task<VendorCommissionAcknowledgement?> GetLatestAcknowledgementAsync(Guid vendorProfileId, CancellationToken ct = default);
}
