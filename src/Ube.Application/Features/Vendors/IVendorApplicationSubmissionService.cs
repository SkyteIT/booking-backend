namespace Ube.Application.Features.Vendors;

public interface IVendorApplicationSubmissionService
{
    Task<Guid> SubmitAsync(Guid userId, SubmitVendorApplicationRequest request, CancellationToken cancellationToken);
}
