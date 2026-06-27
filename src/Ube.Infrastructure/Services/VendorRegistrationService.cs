using Microsoft.Extensions.Logging;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.VendorRegistration;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Vendors;

namespace Ube.Infrastructure.Services;

public class VendorRegistrationService : IVendorRegistrationService
{
    private readonly IVendorApplicationRepository _repo;
    private readonly ILogger<VendorRegistrationService> _logger;

    public VendorRegistrationService(
        IVendorApplicationRepository repo,
        ILogger<VendorRegistrationService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Guid> SubmitApplicationAsync(
        Guid userId,
        VendorRegisterApplicationDto dto,
        Stream? businessLicense, string? businessLicenseExt,
        Stream? insuranceCertificate, string? insuranceCertificateExt,
        Stream? taxDocument, string? taxDocumentExt)
    {
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(uploadPath);

        var application = new VendorApplication
        {
            UserId = userId,
            BusinessName = dto.BusinessName,
            BusinessType = dto.BusinessType,
            Address = dto.Address,
            Website = dto.Website,
            TaxId = dto.TaxId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Categories = dto.Categories.Count > 0 ? string.Join(",", dto.Categories) : null,
            BusinessLicensePath = await SaveFileAsync(uploadPath, businessLicense, businessLicenseExt),
            InsuranceCertificatePath = await SaveFileAsync(uploadPath, insuranceCertificate, insuranceCertificateExt),
            TaxDocumentPath = await SaveFileAsync(uploadPath, taxDocument, taxDocumentExt),
            CurrentStep = dto.CurrentStep,
            Status = VendorApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(application);

        _logger.LogInformation("Vendor application {ApplicationId} submitted by user {UserId}", application.Id, userId);

        return application.Id;
    }

    private static async Task<string?> SaveFileAsync(string uploadPath, Stream? stream, string? extension)
    {
        if (stream == null) return null;

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        return fileName;
    }
}
