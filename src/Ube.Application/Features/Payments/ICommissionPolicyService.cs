namespace Ube.Application.Features.Payments;

public interface ICommissionPolicyService
{
    Task<VendorCommissionOverrideDto> CreateOverrideAsync(Guid actorUserId, CreateCommissionOverrideRequest request, CancellationToken ct = default);
    Task<VendorCommissionOverrideDto> RevokeOverrideAsync(Guid actorUserId, Guid overrideId, CancellationToken ct = default);
    Task<IReadOnlyList<VendorCommissionOverrideDto>> GetOverridesForVendorAsync(Guid vendorProfileId, CancellationToken ct = default);

    Task<LoyaltyDiscountTierDto> CreateLoyaltyTierAsync(Guid actorUserId, CreateLoyaltyTierRequest request, CancellationToken ct = default);
    Task<LoyaltyDiscountTierDto> UpdateLoyaltyTierAsync(Guid actorUserId, Guid tierId, UpdateLoyaltyTierRequest request, CancellationToken ct = default);
    Task DeleteLoyaltyTierAsync(Guid actorUserId, Guid tierId, CancellationToken ct = default);
    Task<IReadOnlyList<LoyaltyDiscountTierDto>> GetLoyaltyTiersAsync(CancellationToken ct = default);
}
