namespace Ube.Application.Features.Payments;

public record CommissionResolution(decimal CommissionPercent, string Source, Guid? OverrideId = null);

// Resolves the commission percent a payment should use, in the documented
// precedence order: vendor-specific override -> loyalty tier -> category
// default. Called once per Payment at charge time; the result is
// snapshotted onto the Payment row and never recalculated afterward.
public interface ICommissionResolverService
{
    Task<CommissionResolution> ResolveAsync(Guid vendorProfileId, Guid categoryId, DateTime asOf, CancellationToken ct = default);
}
