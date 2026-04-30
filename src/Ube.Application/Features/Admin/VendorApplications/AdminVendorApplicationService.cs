
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Models;
using Ube.Application.Common.Models.Pagination;



namespace Ube.Application.Features.Admin.VendorApplications;

public class AdminVendorApplicationService : IAdminVendorApplicationService
{
    private readonly IVendorApplicationRepository _applicationRepo;
    private readonly IUserRepository _userRepo;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly IUnitOfWork _unitOfWork;
    
    public AdminVendorApplicationService(IVendorApplicationRepository applicationRepo, IUserRepository userRepo, IVendorProfileRepository vendorRepo, IUnitOfWork unitOfWork)
    {
        _applicationRepo = applicationRepo;
        _userRepo = userRepo;
        _vendorRepo = vendorRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task ReviewApplicationAsync(Guid applicationId,Guid adminId, ReviewVendorApplicationDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try{
        // Get application
            var application = await _applicationRepo.GetByIdAsync(applicationId);
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
            throw;
        }
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
    public async Task<PagedResult<ApplicationDetailDto>> GetAllAsync(VendorApplicationStatus? status, VendorApplicationsRequest request)
    {
        var (apps, totalItems) = await _applicationRepo.GetPagedAsync(status, request);
        var mapped = apps.Select(app => new ApplicationDetailDto
        {
            Id = app.Id,
            BusinessName = app.BusinessName,
            BusinessType = app.BusinessType,
            ContactNumber = app.ContactNumber,
            Status = app.Status,
            SubmittedAt = app.SubmittedAt,
            ReviewedAt = app.ReviewedAt,
            ReviewedBy = app.ReviewedBy,
        }).ToList();
        
        return new PagedResult<ApplicationDetailDto>
        {
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }
}