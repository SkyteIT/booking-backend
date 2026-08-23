using Microsoft.Extensions.Logging;
using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Admin.VendorApplications;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;

namespace Ube.Tests.Vendors;

public class AdminVendorApplicationServiceTests
{
    [Fact]
    public async Task ReviewApplicationAsync_Accepts_String_Approved_Status()
    {
        var service = BuildService(
            application: CreateApplication(),
            user: CreateApplicant());

        var dto = new ReviewVendorApplicationDto
        {
            Status = "Approved"
        };

        await service.Service.ReviewApplicationAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.NewGuid(), dto);

        service.ApplicationRepo.Verify(x => x.UpdateAsync(It.Is<VendorApplication>(a => a.Status == VendorApplicationStatus.Approved)), Times.Once);
        service.UserRepo.Verify(x => x.UpdateAsync(It.Is<User>(u => u.Role == UserRole.Vendor)), Times.Once);
        service.VendorRepo.Verify(x => x.AddAsync(It.Is<VendorProfile>(v => v.UserId == service.Applicant.Id)), Times.Once);
        service.EmailService.Verify(x => x.SendVendorApplicationApprovedEmailAsync(
            service.Application.Email,
            service.Applicant.FirstName,
            service.Application.BusinessName), Times.Once);
    }

    [Fact]
    public async Task ReviewApplicationAsync_Accepts_Lowercase_Reject_Status()
    {
        var service = BuildService(
            application: CreateApplication(),
            user: CreateApplicant());

        var dto = new ReviewVendorApplicationDto
        {
            Status = "reject",
            RejectionReason = "Documents are incomplete"
        };

        await service.Service.ReviewApplicationAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.NewGuid(), dto);

        service.ApplicationRepo.Verify(x => x.UpdateAsync(It.Is<VendorApplication>(a =>
            a.Status == VendorApplicationStatus.Rejected &&
            a.RejectionReason == "Documents are incomplete")), Times.Once);
        service.VendorRepo.Verify(x => x.AddAsync(It.IsAny<VendorProfile>()), Times.Never);
        service.UserRepo.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        service.EmailService.Verify(x => x.SendVendorApplicationRejectedEmailAsync(
            service.Application.Email,
            service.Applicant.FirstName,
            service.Application.BusinessName,
            "Documents are incomplete"), Times.Once);
    }

    [Fact]
    public async Task ReviewApplicationAsync_Rejects_Without_Rejection_Reason_And_Sends_Generic_Email()
    {
        var service = BuildService(
            application: CreateApplication(),
            user: CreateApplicant());

        var dto = new ReviewVendorApplicationDto
        {
            Status = "reject"
        };

        await service.Service.ReviewApplicationAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.NewGuid(), dto);

        service.ApplicationRepo.Verify(x => x.UpdateAsync(It.Is<VendorApplication>(a =>
            a.Status == VendorApplicationStatus.Rejected &&
            a.RejectionReason == null)), Times.Once);
        service.EmailService.Verify(x => x.SendVendorApplicationRejectedEmailAsync(
            service.Application.Email,
            service.Applicant.FirstName,
            service.Application.BusinessName,
            null), Times.Once);
    }

    [Fact]
    public async Task ReviewApplicationAsync_Accepts_Action_Field()
    {
        var service = BuildService(
            application: CreateApplication(),
            user: CreateApplicant());

        var dto = new ReviewVendorApplicationDto
        {
            Action = "approve"
        };

        await service.Service.ReviewApplicationAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.NewGuid(), dto);

        service.ApplicationRepo.Verify(x => x.UpdateAsync(It.Is<VendorApplication>(a => a.Status == VendorApplicationStatus.Approved)), Times.Once);
        service.EmailService.Verify(x => x.SendVendorApplicationApprovedEmailAsync(
            service.Application.Email,
            service.Applicant.FirstName,
            service.Application.BusinessName), Times.Once);
    }

    [Fact]
    public async Task ReviewApplicationAsync_Sends_Review_Email_To_Distinct_Application_And_User_Emails()
    {
        var service = BuildService(
            application: CreateApplication(),
            user: CreateApplicant("customer@example.com"));

        var dto = new ReviewVendorApplicationDto
        {
            Status = "reject"
        };

        await service.Service.ReviewApplicationAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.NewGuid(), dto);

        service.EmailService.Verify(x => x.SendVendorApplicationRejectedEmailAsync(
            service.Application.Email,
            service.Applicant.FirstName,
            service.Application.BusinessName,
            null), Times.Once);
        service.EmailService.Verify(x => x.SendVendorApplicationRejectedEmailAsync(
            "customer@example.com",
            service.Applicant.FirstName,
            service.Application.BusinessName,
            null), Times.Once);
    }

    private static AdminVendorApplicationServiceHarness BuildService(VendorApplication application, User user)
    {
        var applicationRepo = new Mock<IVendorApplicationRepository>();
        var userRepo = new Mock<IUserRepository>();
        var vendorRepo = new Mock<IVendorProfileRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var encryptionService = new Mock<IEncryptionService>();
        var emailService = new Mock<IEmailService>();
        var notificationService = new Mock<INotificationService>();
        var realtimeService = new Mock<IRealtimeUpdateService>();
        var fileStorage = new Mock<IFileStorageService>();
        var logger = new Mock<ILogger<AdminVendorApplicationService>>();

        applicationRepo.Setup(x => x.GetByIdAsync(application.Id)).ReturnsAsync(application);
        applicationRepo.Setup(x => x.UpdateAsync(It.IsAny<VendorApplication>())).Returns(Task.CompletedTask);
        userRepo.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        userRepo.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        userRepo.Setup(x => x.GetByRoleAsync(UserRole.Admin)).ReturnsAsync(Array.Empty<User>());
        vendorRepo.Setup(x => x.GetVendorIdAsync(user.Id)).ReturnsAsync((VendorProfile?)null);
        vendorRepo.Setup(x => x.AddAsync(It.IsAny<VendorProfile>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
        notificationService
            .Setup(x => x.CreateAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateNotificationDto dto, CancellationToken _) => new NotificationDto
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type.ToString()
            });
        realtimeService.Setup(x => x.PublishToRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new AdminVendorApplicationServiceHarness(
            new AdminVendorApplicationService(
                applicationRepo: applicationRepo.Object,
                userRepo: userRepo.Object,
                vendorRepo: vendorRepo.Object,
                unitOfWork: unitOfWork.Object,
                encryptionService: encryptionService.Object,
                emailService: emailService.Object,
                notificationService: notificationService.Object,
                realtimeUpdateService: realtimeService.Object,
                fileStorage: fileStorage.Object,
                logger: logger.Object),
            applicationRepo,
            userRepo,
            vendorRepo,
            emailService,
            application,
            user);
    }

    private static VendorApplication CreateApplication() => new()
    {
        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        BusinessName = "Blue Lagoon Tours",
        BusinessType = "Tours",
        Description = "Tour operator",
        Address = "123 Galle Road",
        FirstName = "Asha",
        LastName = "Perera",
        Phone = "0771234567",
        BusinessLicensePath = "/uploads/license.pdf",
        InsuranceCertificatePath = "/uploads/insurance.pdf",
        TaxDocumentPath = "/uploads/tax.pdf",
        Email = "vendor@example.com",
        Status = VendorApplicationStatus.Pending
    };

    private static User CreateApplicant(string email = "vendor@example.com") => new()
    {
        Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
        Email = email,
        FirstName = "Asha",
        LastName = "Perera",
        Role = UserRole.User
    };

    private sealed record AdminVendorApplicationServiceHarness(
        AdminVendorApplicationService Service,
        Mock<IVendorApplicationRepository> ApplicationRepo,
        Mock<IUserRepository> UserRepo,
        Mock<IVendorProfileRepository> VendorRepo,
        Mock<IEmailService> EmailService,
        VendorApplication Application,
        User Applicant);
}
