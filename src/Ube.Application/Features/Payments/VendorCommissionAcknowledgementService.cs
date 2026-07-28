using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public class VendorCommissionAcknowledgementService : IVendorCommissionAcknowledgementService
{
    private readonly IVendorCommissionRepository _commissionRepo;
    private readonly ICommissionResolverService _resolver;

    public VendorCommissionAcknowledgementService(
        IVendorCommissionRepository commissionRepo,
        ICommissionResolverService resolver)
    {
        _commissionRepo = commissionRepo;
        _resolver = resolver;
    }

    public async Task<CommissionAcknowledgementDto> AcknowledgeAsync(Guid vendorProfileId, AcknowledgeCommissionRateRequest request, CancellationToken ct = default)
    {
        var resolution = await _resolver.ResolveAsync(vendorProfileId, request.CategoryId, DateTime.UtcNow, ct);

        var acknowledgement = new VendorCommissionAcknowledgement
        {
            Id = Guid.NewGuid(),
            VendorProfileId = vendorProfileId,
            CommissionPercentShown = resolution.CommissionPercent,
            SourceOverrideId = resolution.OverrideId
        };

        await _commissionRepo.AddAcknowledgementAsync(acknowledgement, ct);

        return ToDto(acknowledgement);
    }

    public async Task<IReadOnlyList<CommissionAcknowledgementDto>> GetHistoryAsync(Guid vendorProfileId, CancellationToken ct = default)
    {
        var history = await _commissionRepo.GetAllAcknowledgementsForVendorAsync(vendorProfileId, ct);
        return history.Select(ToDto).ToList();
    }

    private static CommissionAcknowledgementDto ToDto(VendorCommissionAcknowledgement a) => new()
    {
        Id = a.Id,
        VendorProfileId = a.VendorProfileId,
        CommissionPercentShown = a.CommissionPercentShown,
        SourceOverrideId = a.SourceOverrideId,
        AcknowledgedAt = a.AcknowledgedAt
    };
}
