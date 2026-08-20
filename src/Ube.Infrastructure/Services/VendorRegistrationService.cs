using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Features.VendorRegistration;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;

namespace Ube.Infrastructure.Services;

public class VendorRegistrationService : IVendorRegistrationService
{
    private readonly IVendorApplicationRepository _repo;
    private readonly IAdminAlertService _adminAlertService;
    private readonly IEncryptionService _encryptionService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<VendorRegistrationService> _logger;

    public VendorRegistrationService(
        IVendorApplicationRepository repo,
        IAdminAlertService adminAlertService,
        IEncryptionService encryptionService,
        IEmailService emailService,
        IWebHostEnvironment environment,
        ILogger<VendorRegistrationService> logger)
    {
        _repo = repo;
        _adminAlertService = adminAlertService;
        _encryptionService = encryptionService;
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<Guid> SubmitApplicationAsync(
        Guid userId,
        VendorRegisterApplicationDto dto,
        Stream? businessLicense, string? businessLicenseExt,
        Stream? insuranceCertificate, string? insuranceCertificateExt,
        Stream? taxDocument, string? taxDocumentExt)
    {
        // A rejected applicant is allowed to re-apply; anyone with a Pending
        // or already-Approved application is not - prevents duplicate/spam
        // submissions that GetMyStatusAsync would otherwise silently overwrite
        // the view of (it only ever surfaces the latest one).
        var existing = await _repo.GetLatestByUserIdAsync(userId);
        if (existing != null && existing.Status != VendorApplicationStatus.Rejected)
        {
            throw new BusinessRuleException("You already have a vendor application in progress.");
        }

        // wwwroot so UseStaticFiles() actually serves these - the old
        // Uploads/ path sat outside wwwroot and every document link 404'd.
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadPath = Path.Combine(webRoot, "uploads", "vendor-applications");
        Directory.CreateDirectory(uploadPath);

        var application = new VendorApplication
        {
            UserId = userId,
            BusinessName = dto.BusinessName,
            BusinessType = dto.BusinessType,
            Address = dto.Address,
            Website = dto.Website,
            TaxId = string.IsNullOrEmpty(dto.TaxId) ? dto.TaxId : _encryptionService.Encrypt(dto.TaxId),
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

        await _adminAlertService.NotifyRolesAsync(
            new[] { UserRole.Admin, UserRole.SuperAdmin },
            "New vendor application submitted",
            $"{application.BusinessName} has applied to become a vendor and is awaiting review.",
            NotificationType.VendorApplicationSubmitted);

        try
        {
            await _emailService.SendVendorApplicationSubmittedEmailAsync(
                application.Email, application.FirstName, application.BusinessName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send vendor application confirmation email to {Email} for application {ApplicationId}", application.Email, application.Id);
        }

        return application.Id;
    }

    public async Task<MyVendorApplicationStatusDto?> GetMyStatusAsync(Guid userId)
    {
        var application = await _repo.GetLatestByUserIdAsync(userId);
        if (application == null) return null;

        return new MyVendorApplicationStatusDto
        {
            Id = application.Id,
            BusinessName = application.BusinessName,
            Status = application.Status,
            SubmittedAt = application.SubmittedAt,
            ReviewedAt = application.ReviewedAt,
            RejectionReason = application.RejectionReason
        };
    }

    private static async Task<string?> SaveFileAsync(string uploadPath, Stream? stream, string? extension)
    {
        if (stream == null) return null;

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        return $"/uploads/vendor-applications/{fileName}";
    }
}
