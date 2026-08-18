using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.DTOs.Notification;
using Ube.Application.Features.Admin.VendorApplications;
using Ube.Application.Features.Vendors;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;

namespace Ube.Infrastructure.Services.Vendors;

public class VendorApplicationSubmissionService : IVendorApplicationSubmissionService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".doc",
        ".docx"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IUserRepository _userRepository;
    private readonly IVendorApplicationRepository _applicationRepository;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeUpdateService _realtimeUpdateService;
    private readonly ILogger<VendorApplicationSubmissionService> _logger;

    public VendorApplicationSubmissionService(
        IWebHostEnvironment environment,
        IUserRepository userRepository,
        IVendorApplicationRepository applicationRepository,
        INotificationService notificationService,
        IRealtimeUpdateService realtimeUpdateService,
        ILogger<VendorApplicationSubmissionService> logger)
    {
        _environment = environment;
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _notificationService = notificationService;
        _realtimeUpdateService = realtimeUpdateService;
        _logger = logger;
    }

    public async Task<Guid> SubmitAsync(Guid userId, SubmitVendorApplicationRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (user.Role == UserRole.Vendor)
            throw new BusinessRuleException("User is already a vendor");

        var existingApplication = await _applicationRepository.GetByUserIdAsync(userId);
        if (existingApplication?.Status == VendorApplicationStatus.Pending)
            throw new BusinessRuleException("Vendor application already submitted and pending review");

        var applicationId = existingApplication?.Id ?? Guid.NewGuid();
        var uploadsRoot = GetUploadsRoot(applicationId);

        var businessLicenseUrl = await SaveDocumentAsync(request.BusinessLicense, uploadsRoot, "business-license", cancellationToken);
        var insuranceCertificateUrl = await SaveDocumentAsync(request.InsuranceCertificate, uploadsRoot, "insurance-certificate", cancellationToken);
        var taxDocumentUrl = await SaveDocumentAsync(request.TaxDocument, uploadsRoot, "tax-document", cancellationToken);

        var description = BuildDescription(request);

        var application = existingApplication ?? new VendorApplication
        {
            Id = applicationId,
            UserId = userId,
            SubmittedAt = DateTime.UtcNow
        };

        application.BusinessName = request.BusinessName.Trim();
        application.BusinessType = request.BusinessType.Trim();
        application.Description = description;
        application.Address = request.Address.Trim();
        application.ContactPersonName = $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim();
        application.ContactNumber = request.Phone.Trim();
        application.BusinessLicenseUrl = businessLicenseUrl;
        application.InsurenceCertificateUrl = insuranceCertificateUrl;
        application.TaxDocumentUrl = taxDocumentUrl;
        application.Status = VendorApplicationStatus.Pending;
        application.SubmittedAt = DateTime.UtcNow;
        application.ReviewedAt = null;
        application.ReviewedBy = null;
        application.RejectionReason = null;

        if (existingApplication is null)
        {
            await _applicationRepository.AddAsync(application);
        }
        else
        {
            await _applicationRepository.UpdateAsync(application);
        }

        await NotifyAdminsAsync(application, cancellationToken);
        await PublishDashboardRefreshAsync(application, cancellationToken);

        return application.Id;
    }

    private string GetUploadsRoot(Guid applicationId)
    {
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var root = Path.Combine(webRoot, "uploads", "vendor-applications", applicationId.ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static string BuildDescription(SubmitVendorApplicationRequest request)
    {
        var sections = new List<string>();

        if (request.Categories.Count > 0)
            sections.Add($"Categories: {string.Join(", ", request.Categories)}");

        if (!string.IsNullOrWhiteSpace(request.TaxId))
            sections.Add($"Tax ID: {request.TaxId.Trim()}");

        if (!string.IsNullOrWhiteSpace(request.Website))
            sections.Add($"Website: {request.Website.Trim()}");

        if (!string.IsNullOrWhiteSpace(request.Email))
            sections.Add($"Contact email: {request.Email.Trim()}");

        if (sections.Count == 0)
            sections.Add("Vendor application submitted through the web form.");

        return string.Join(Environment.NewLine, sections);
    }

    private static async Task<string> SaveDocumentAsync(
        IFormFile file,
        string uploadsRoot,
        string filePrefix,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new BusinessRuleException($"Invalid {filePrefix.Replace('-', ' ')} file");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            throw new BusinessRuleException($"Unsupported file type for {filePrefix.Replace('-', ' ')}");

        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new BusinessRuleException($"File size for {filePrefix.Replace('-', ' ')} must not exceed 10MB");

        var safeExtension = extension.ToLowerInvariant();
        var fileName = $"{filePrefix}-{Guid.NewGuid():N}{safeExtension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/vendor-applications/{Path.GetFileName(uploadsRoot)}/{fileName}";
    }

    private async Task NotifyAdminsAsync(VendorApplication application, CancellationToken cancellationToken)
    {
        var admins = await _userRepository.GetByRoleAsync(UserRole.Admin);

        foreach (var admin in admins)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = admin.Id,
                    Title = "Vendor application submitted",
                    Message = $"New vendor application submitted: {application.BusinessName}",
                    Type = (int)NotificationType.AdminVendorRequestApproval
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify admin {AdminId} about vendor application {ApplicationId}", admin.Id, application.Id);
            }
        }
    }

    private async Task PublishDashboardRefreshAsync(VendorApplication application, CancellationToken cancellationToken)
    {
        try
        {
            await _realtimeUpdateService.PublishToRoleAsync("admin", "dashboard.refresh", new
            {
                reason = "vendor.application.submitted",
                applicationId = application.Id,
                userId = application.UserId,
                businessName = application.BusinessName
            });
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to publish dashboard refresh for vendor application {ApplicationId}", application.Id);
        }
    }
}
