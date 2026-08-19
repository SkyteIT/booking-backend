using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Interfaces;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Vendors;
using Ube.Infrastructure.Services.Vendors;

namespace Ube.Tests.Vendors;

public class VendorApplicationSubmissionServiceTests
{
    [Fact]
    public async Task SubmitAsync_Sends_Confirmation_Email_To_Customer()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.WebRootPath).Returns(tempRoot);
            environment.SetupGet(x => x.ContentRootPath).Returns(tempRoot);

            var userId = Guid.NewGuid();
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User
            {
                Id = userId,
                Email = "customer@example.com",
                FirstName = "Asha",
                LastName = "Perera",
                Role = UserRole.User
            });
            userRepo.Setup(r => r.GetByRoleAsync(UserRole.Admin)).ReturnsAsync(Array.Empty<User>());

            var applicationRepo = new Mock<IVendorApplicationRepository>();
            applicationRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Domain.Entities.Vendors.VendorApplication?)null);
            applicationRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Vendors.VendorApplication>())).Returns(Task.CompletedTask);
            applicationRepo.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.Vendors.VendorApplication>())).Returns(Task.CompletedTask);

            var emailService = new Mock<IEmailService>();
            var notificationService = new Mock<INotificationService>();
            var realtimeService = new Mock<IRealtimeUpdateService>();
            var logger = new Mock<ILogger<VendorApplicationSubmissionService>>();

            notificationService
                .Setup(x => x.CreateAsync(It.IsAny<Ube.Application.DTOs.Notification.CreateNotificationDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ube.Application.DTOs.Notification.CreateNotificationDto dto, CancellationToken _) => new Ube.Application.DTOs.Notification.NotificationDto
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    Title = dto.Title,
                    Message = dto.Message,
                    Type = dto.Type.ToString()
                });

            realtimeService.Setup(x => x.PublishToRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailService
                .Setup(x => x.SendVendorApplicationSubmittedEmailAsync(
                    "customer@example.com",
                    "Asha",
                    "Blue Lagoon Tours"))
                .Returns(Task.CompletedTask);

            var service = new VendorApplicationSubmissionService(
                environment.Object,
                userRepo.Object,
                applicationRepo.Object,
                emailService.Object,
                notificationService.Object,
                realtimeService.Object,
                logger.Object);

            var request = new SubmitVendorApplicationRequest
            {
                BusinessName = "Blue Lagoon Tours",
                BusinessType = "Tours",
                Address = "123 Galle Road",
                FirstName = "Asha",
                LastName = "Perera",
                Email = "customer@example.com",
                Phone = "0771234567",
                BusinessLicense = CreateFile("business-license.pdf"),
                InsuranceCertificate = CreateFile("insurance-certificate.pdf"),
                TaxDocument = CreateFile("tax-document.pdf")
            };

            var result = await service.SubmitAsync(userId, request, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, result);
            emailService.Verify(x => x.SendVendorApplicationSubmittedEmailAsync(
                "customer@example.com",
                "Asha",
                "Blue Lagoon Tours"), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, true);
        }
    }

    private static IFormFile CreateFile(string fileName)
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };
    }
}
