using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Vendors;

namespace Ube.Application.Features.Payments;

public class CommissionResolverService : ICommissionResolverService
{
    private readonly IVendorCommissionRepository _commissionRepo;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly ICategoryRepository _categoryRepo;

    public CommissionResolverService(
        IVendorCommissionRepository commissionRepo,
        IVendorProfileRepository vendorRepo,
        ICategoryRepository categoryRepo)
    {
        _commissionRepo = commissionRepo;
        _vendorRepo = vendorRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<CommissionResolution> ResolveAsync(Guid vendorProfileId, Guid categoryId, DateTime asOf, CancellationToken ct = default)
    {
        // 1. Vendor-specific override - highest priority.
        var activeOverride = await _commissionRepo.GetActiveOverrideAsync(vendorProfileId, categoryId, asOf, ct);
        if (activeOverride != null)
            return new CommissionResolution(activeOverride.CommissionPercent, "VendorOverride");

        var category = await _categoryRepo.GetByIdAsync(categoryId, ct: ct)
            ?? throw new NotFoundException("Category not found");

        // 2. Loyalty tier - automatic, based purely on tenure.
        var vendor = await _vendorRepo.GetByIdAsync(vendorProfileId)
            ?? throw new NotFoundException("Vendor profile not found");

        var tenureMonths = ((asOf.Year - vendor.CreatedAt.Year) * 12) + asOf.Month - vendor.CreatedAt.Month;
        var tiers = await _commissionRepo.GetLoyaltyTiersAsync(ct);
        var qualifyingTier = tiers
            .Where(t => t.MonthsActive <= tenureMonths)
            .OrderByDescending(t => t.MonthsActive)
            .FirstOrDefault();

        if (qualifyingTier != null)
        {
            var discounted = Math.Max(0, category.DefaultCommissionPercent - qualifyingTier.DiscountPercent);
            return new CommissionResolution(discounted, "LoyaltyTier");
        }

        // 3. Category default - fallback.
        return new CommissionResolution(category.DefaultCommissionPercent, "CategoryDefault");
    }
}
