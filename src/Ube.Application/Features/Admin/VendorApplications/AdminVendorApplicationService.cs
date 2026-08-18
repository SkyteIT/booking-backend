
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Models;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.DTOs.Notification;
using Ube.Application.Interfaces;
using Ube.Domain.Enums.Notifications;
using Ube.Application.Common.Interfaces.Services;



namespace Ube.Application.Features.Admin.VendorApplications;

public class AdminVendorApplicationService : IAdminVendorApplicationService
{
    private readonly IVendorApplicationRepository _applicationRepo;
    private readonly IUserRepository _userRepo;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeUpdateService _realtimeUpdateService;
    
    public AdminVendorApplicationService(
        IVendorApplicationRepository applicationRepo,
        IUserRepository userRepo,
        IVendorProfileRepository vendorRepo,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IRealtimeUpdateService realtimeUpdateService)
    {
        _applicationRepo = applicationRepo;
        _userRepo = userRepo;
        _vendorRepo = vendorRepo;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _realtimeUpdateService = realtimeUpdateService;
    }

    public async Task ReviewApplicationAsync(Guid applicationId,Guid adminId, ReviewVendorApplicationDto dto)
    {
        Domain.Entities.Vendors.VendorApplication? application = null;
        Domain.Entities.Users.User? user = null;

        // traaction
        await _unitOfWork.BeginTransactionAsync();
        try{
        // Get application
            application = await _applicationRepo.GetByIdAsync(applicationId);
            if (application == null)
                throw new NotFoundException("Application not found");

            //  Rule: Only Pending can be reviewed
            var selfReviewRule = VendorApplicationRules.CannotBeAdmin(application.UserId, adminId);
            if (!selfReviewRule.IsSuccess)
                throw new BusinessRuleException(selfReviewRule.ErrorMessage);
            var reviewRule = VendorApplicationRules.CanReview(application.Status);
            if (!reviewRule.IsSuccess)
                throw new BusinessRuleException(reviewRule.ErrorMessage);

            // Get user
            user = await _userRepo.GetByIdAsync(application.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            // Approval Flow
            if (dto.Status == VendorApplicationStatus.Approved)
            {
                var existingVendor = await _vendorRepo.GetVendorIdAsync(user.Id);

                //Rule: Validate approval
                var approvalRule = VendorApplicationRules.ValidateApproval(
                    user.Role == UserRole.Vendor,
                    existingVendor != null
                );

                if (!approvalRule.IsSuccess)
                    throw new BusinessRuleException(approvalRule.ErrorMessage);

                //Update application
                application.Status = VendorApplicationStatus.Approved;
                application.ReviewedAt = DateTime.UtcNow;
                application.ReviewedBy = adminId;

                // Create VendorProfile
                var vendorProfile = new VendorProfile
                {   
                    //from user
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    //from application
                    BusinessName = application.BusinessName,
                    BusinessType = application.BusinessType,
                    BusinessDescription = application.Description,
                    ContactNumber = application.ContactNumber,

                    //default values
                    Bio = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _vendorRepo.AddAsync(vendorProfile);

                // Update user role
                user.Role = UserRole.Vendor;
                await _userRepo.UpdateAsync(user);
            }

            // Rejection Flow
            else if (dto.Status == VendorApplicationStatus.Rejected)
            {
                //Rule: Validate rejection
                var rejectRule = VendorApplicationRules.ValidateRejection(dto.RejectionReason);
                if (!rejectRule.IsSuccess)
                    throw new BusinessRuleException(rejectRule.ErrorMessage);

                //Update application
                application.Status = VendorApplicationStatus.Rejected;
                application.ReviewedAt = DateTime.UtcNow;
                application.ReviewedBy = adminId;
                application.RejectionReason = dto.RejectionReason;
            }

            // Invalid status
            else
            {
                throw new BusinessRuleException("Invalid application status");
            }

            // Save application
            await _applicationRepo.UpdateAsync(application);
            // Commit transaction
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            // Rollback transaction on error
            await _unitOfWork.RollbackAsync();
            throw new BusinessRuleException("An error occurred while reviewing the application");
        }

        if (application is null || user is null)
            return;

        await NotifyApplicationReviewedAsync(application, user, dto.Status);
        await PublishDashboardRefreshAsync(application.Id, user.Id, dto.Status.ToString());
    }
    // Method to get application details
    public async Task<ApplicationDetailDto> GetDetailsAsync(Guid applicationId)
    {
        var app = await _applicationRepo.GetByIdAsync(applicationId);
        if(app == null)
            throw new NotFoundException("Application not found");

        return new ApplicationDetailDto
        {
            Id = app.Id,
            UserId = app.UserId,
            BusinessName = app.BusinessName,
            BusinessType = app.BusinessType,
            Description = app.Description,
            Address = app.Address,
            ContactPersonName = app.ContactPersonName,
            ContactNumber = app.ContactNumber,
            BusinessLicenseUrl = app.BusinessLicenseUrl,
            InsurenceCertificateUrl = app.InsurenceCertificateUrl,
            TaxDocumentUrl = app.TaxDocumentUrl,
            Status = app.Status,
            SubmittedAt = app.SubmittedAt,
            ReviewedAt = app.ReviewedAt,
            ReviewedBy = app.ReviewedBy,
            RejectionReason = app.RejectionReason
        };
    
    }
    // Method to get all applications (for admin listing)
    public async Task<PagedResult<ApplicationTableDto>> GetAllAsync(VendorApplicationStatus? status, VendorApplicationsRequest request)
    {
        var (mapped, totalItems) = await _applicationRepo.GetPagedTableAsync(status, request);
        
        return new PagedResult<ApplicationTableDto>
        {
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }

    private async Task NotifyApplicationReviewedAsync(
        Domain.Entities.Vendors.VendorApplication application,
        Domain.Entities.Users.User user,
        VendorApplicationStatus status)
    {
        var notifications = new List<(Guid UserId, NotificationType Type, string Title, string Message)>();

        if (status == VendorApplicationStatus.Approved)
        {
            notifications.Add((user.Id, NotificationType.VendorAccountApproved, "Vendor account approved",
                "Your vendor application has been approved."));
            notifications.AddRange((await _userRepo.GetByRoleAsync(UserRole.Admin))
                .Select(admin => (admin.Id, NotificationType.AdminNewVendorRegistered, "New vendor registered",
                    $"Vendor account approved: {application.BusinessName}")));
        }
        else if (status == VendorApplicationStatus.Rejected)
        {
            notifications.Add((user.Id, NotificationType.VendorAccountRejected, "Vendor account rejected",
                "Your vendor application has been rejected."));
        }

        foreach (var notification in notifications)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = notification.UserId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = (int)notification.Type
                }, CancellationToken.None);
            }
            catch
            {
                // best-effort only
            }
        }
    }

    private async Task PublishDashboardRefreshAsync(Guid applicationId, Guid userId, string status)
    {
        try
        {
            var payload = new
            {
                reason = "vendor.application.reviewed",
                applicationId,
                userId,
                status
            };

            await _realtimeUpdateService.PublishToRoleAsync("admin", "dashboard.refresh", payload);
            await _realtimeUpdateService.PublishToRoleAsync("vendor", "dashboard.refresh", payload);
        }
        catch
        {
            // Realtime refresh is best-effort.
        }
    }
}
