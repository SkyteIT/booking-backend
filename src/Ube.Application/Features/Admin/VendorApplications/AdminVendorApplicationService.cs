
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;

namespace Ube.Application.Features.Admin.VendorApplications;

public class AdminVendorApplicationService : IAdminVendorApplicationService
{
    private readonly IVendorApplicationRepository _applicationRepo;
    private readonly IUserRepository _userRepo;
    private readonly IVendorProfileRepository _vendorRepo;

    public AdminVendorApplicationService(
        IVendorApplicationRepository applicationRepo,
        IUserRepository userRepo,
        IVendorProfileRepository vendorRepo)
    {
        _applicationRepo = applicationRepo;
        _userRepo = userRepo;
        _vendorRepo = vendorRepo;
    }

    public async Task ReviewApplicationAsync(Guid applicationId,Guid adminId, ReviewVendorApplicationDto dto)
    {
        // Get application
        var application = await _applicationRepo.GetByIdAsync(applicationId);
        if (application == null)
            throw new NotFoundException("Application not found");

        //  Rule: Only Pending can be reviewed
        var reviewRule = VendorApplicationRules.CanReview(application.Status);
        if (!reviewRule.IsSuccess)
            throw new InvalidOperationException(reviewRule.ErrorMessage);

        // Get user
        var user = await _userRepo.GetByIdAsync(application.UserId);
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
                throw new InvalidOperationException(approvalRule.ErrorMessage);

            //Update application
            application.Status = VendorApplicationStatus.Approved;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = adminId;

            // Create VendorProfile
            var vendorProfile = new VendorProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BusinessName = application.BusinessName,
                BusinessType = application.BusinessType,
                BusinessDescription = application.Description,
                ContactNumber = application.ContactNumber,
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
                throw new InvalidOperationException(rejectRule.ErrorMessage);

            //Update application
            application.Status = VendorApplicationStatus.Rejected;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = adminId;
            application.RejectionReason = dto.RejectionReason;
        }

        // Invalid status
        else
        {
            throw new InvalidOperationException("Invalid application status");
        }

        // Save application
        await _applicationRepo.UpdateAsync(application);
    }
}