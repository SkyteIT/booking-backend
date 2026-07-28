using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IPayoutExportSettingsRepository
{
    // Creates the singleton row with the default threshold if it doesn't
    // exist yet - callers never have to worry about a missing settings row.
    Task<PayoutExportSettings> GetOrCreateAsync(CancellationToken ct = default);
    Task UpdateAsync(PayoutExportSettings settings, CancellationToken ct = default);
}
