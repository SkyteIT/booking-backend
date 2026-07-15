using Ube.Application.Features.Bookings;

namespace Ube.Application.Features.VendorRegistration;

public interface IVendorRegistrationService
{
    Task<Guid> SubmitApplicationAsync(Guid userId, VendorRegisterApplicationDto dto,
        Stream? businessLicense, string? businessLicenseExt,
        Stream? insuranceCertificate, string? insuranceCertificateExt,
        Stream? taxDocument, string? taxDocumentExt);
}
