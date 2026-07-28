using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class CommissionPolicyService : ICommissionPolicyService
{
    private readonly IVendorCommissionRepository _commissionRepo;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IPaymentAuditLogRepository _auditRepo;

    public CommissionPolicyService(
        IVendorCommissionRepository commissionRepo,
        IVendorProfileRepository vendorRepo,
        ICategoryRepository categoryRepo,
        IPaymentAuditLogRepository auditRepo)
    {
        _commissionRepo = commissionRepo;
        _vendorRepo = vendorRepo;
        _categoryRepo = categoryRepo;
        _auditRepo = auditRepo;
    }

    public async Task<VendorCommissionOverrideDto> CreateOverrideAsync(Guid actorUserId, CreateCommissionOverrideRequest request, CancellationToken ct = default)
    {
        if (request.CommissionPercent < 0 || request.CommissionPercent > 100)
            throw new BusinessRuleException("Commission percent must be between 0 and 100");

        if (request.EndDate.HasValue && request.EndDate.Value <= request.StartDate)
            throw new BusinessRuleException("End date must be after the start date");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BusinessRuleException("A reason is required for every commission override");

        _ = await _vendorRepo.GetByIdAsync(request.VendorProfileId)
            ?? throw new NotFoundException("Vendor profile not found");

        if (request.CategoryId.HasValue)
            _ = await _categoryRepo.GetByIdAsync(request.CategoryId.Value, ct: ct)
                ?? throw new NotFoundException("Category not found");

        var commissionOverride = new VendorCommissionOverride
        {
            Id = Guid.NewGuid(),
            VendorProfileId = request.VendorProfileId,
            CategoryId = request.CategoryId,
            CommissionPercent = request.CommissionPercent,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            CreatedByUserId = actorUserId,
            Status = CommissionOverrideStatus.Active
        };

        await _commissionRepo.AddOverrideAsync(commissionOverride, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "CommissionOverrideCreated",
            EntityType = nameof(VendorCommissionOverride),
            EntityId = commissionOverride.Id,
            MetadataJson = $"{{\"vendorProfileId\":\"{request.VendorProfileId}\",\"commissionPercent\":{request.CommissionPercent}}}"
        }, ct);

        return ToDto(commissionOverride);
    }

    public async Task<VendorCommissionOverrideDto> RevokeOverrideAsync(Guid actorUserId, Guid overrideId, CancellationToken ct = default)
    {
        var commissionOverride = await _commissionRepo.GetOverrideByIdAsync(overrideId, ct)
            ?? throw new NotFoundException("Commission override not found");

        if (commissionOverride.Status != CommissionOverrideStatus.Active)
            throw new BusinessRuleException("Only an active override can be revoked");

        commissionOverride.Status = CommissionOverrideStatus.Revoked;
        commissionOverride.RevokedByUserId = actorUserId;
        commissionOverride.RevokedAt = DateTime.UtcNow;
        await _commissionRepo.UpdateOverrideAsync(commissionOverride, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "CommissionOverrideRevoked",
            EntityType = nameof(VendorCommissionOverride),
            EntityId = commissionOverride.Id
        }, ct);

        return ToDto(commissionOverride);
    }

    public async Task<IReadOnlyList<VendorCommissionOverrideDto>> GetOverridesForVendorAsync(Guid vendorProfileId, CancellationToken ct = default)
    {
        var overrides = await _commissionRepo.GetAllForVendorAsync(vendorProfileId, ct);
        return overrides.Select(ToDto).ToList();
    }

    public async Task<LoyaltyDiscountTierDto> CreateLoyaltyTierAsync(Guid actorUserId, CreateLoyaltyTierRequest request, CancellationToken ct = default)
    {
        if (request.MonthsActive < 0)
            throw new BusinessRuleException("MonthsActive cannot be negative");

        if (request.DiscountPercent < 0 || request.DiscountPercent > 100)
            throw new BusinessRuleException("Discount percent must be between 0 and 100");

        var existing = await _commissionRepo.GetLoyaltyTiersAsync(ct);
        if (existing.Any(t => t.MonthsActive == request.MonthsActive))
            throw new BusinessRuleException("A loyalty tier for this MonthsActive value already exists");

        var tier = new LoyaltyDiscountTier
        {
            Id = Guid.NewGuid(),
            MonthsActive = request.MonthsActive,
            DiscountPercent = request.DiscountPercent
        };

        await _commissionRepo.AddLoyaltyTierAsync(tier, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "LoyaltyTierCreated",
            EntityType = nameof(LoyaltyDiscountTier),
            EntityId = tier.Id
        }, ct);

        return ToDto(tier);
    }

    public async Task<LoyaltyDiscountTierDto> UpdateLoyaltyTierAsync(Guid actorUserId, Guid tierId, UpdateLoyaltyTierRequest request, CancellationToken ct = default)
    {
        if (request.DiscountPercent < 0 || request.DiscountPercent > 100)
            throw new BusinessRuleException("Discount percent must be between 0 and 100");

        var tier = await _commissionRepo.GetLoyaltyTierByIdAsync(tierId, ct)
            ?? throw new NotFoundException("Loyalty tier not found");

        tier.DiscountPercent = request.DiscountPercent;
        tier.UpdatedAt = DateTime.UtcNow;
        await _commissionRepo.UpdateLoyaltyTierAsync(tier, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "LoyaltyTierUpdated",
            EntityType = nameof(LoyaltyDiscountTier),
            EntityId = tier.Id
        }, ct);

        return ToDto(tier);
    }

    public async Task DeleteLoyaltyTierAsync(Guid actorUserId, Guid tierId, CancellationToken ct = default)
    {
        var tier = await _commissionRepo.GetLoyaltyTierByIdAsync(tierId, ct)
            ?? throw new NotFoundException("Loyalty tier not found");

        await _commissionRepo.DeleteLoyaltyTierAsync(tierId, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "LoyaltyTierDeleted",
            EntityType = nameof(LoyaltyDiscountTier),
            EntityId = tier.Id
        }, ct);
    }

    public async Task<IReadOnlyList<LoyaltyDiscountTierDto>> GetLoyaltyTiersAsync(CancellationToken ct = default)
    {
        var tiers = await _commissionRepo.GetLoyaltyTiersAsync(ct);
        return tiers.Select(ToDto).ToList();
    }

    private static VendorCommissionOverrideDto ToDto(VendorCommissionOverride o) => new()
    {
        Id = o.Id,
        VendorProfileId = o.VendorProfileId,
        CategoryId = o.CategoryId,
        CommissionPercent = o.CommissionPercent,
        StartDate = o.StartDate,
        EndDate = o.EndDate,
        Reason = o.Reason,
        Status = o.Status,
        CreatedByUserId = o.CreatedByUserId,
        CreatedAt = o.CreatedAt,
        RevokedByUserId = o.RevokedByUserId,
        RevokedAt = o.RevokedAt
    };

    private static LoyaltyDiscountTierDto ToDto(LoyaltyDiscountTier t) => new()
    {
        Id = t.Id,
        MonthsActive = t.MonthsActive,
        DiscountPercent = t.DiscountPercent,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
