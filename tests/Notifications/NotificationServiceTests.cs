using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Notifications.Email;
using Microsoft.Extensions.Logging;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Tests.Notifications;

public class NotificationServiceTests
{
    private static NotificationService BuildService(
        Mock<INotificationRepository> repo,
        Mock<IUserRepository>? userRepo = null)
    {
        userRepo ??= new Mock<IUserRepository>();
        var email = new Mock<IEmailService>();
        var sms = new Mock<ISmsService>();
        var push = new Mock<IPushService>();
        var realtime = new Mock<IRealtimeUpdateService>();
        var logger = new Mock<ILogger<NotificationService>>();
        realtime.Setup(x => x.PublishToUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new NotificationService(repo.Object, userRepo.Object, email.Object, sms.Object, push.Object, realtime.Object, logger.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_BusinessRuleException_For_Invalid_Type()
    {
        var repo = new Mock<INotificationRepository>();
        var service = BuildService(repo);

        var dto = new CreateNotificationDto
        {
            UserId = Guid.NewGuid(),
            Title = "Test",
            Message = "Body",
            Type = 0
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(dto, CancellationToken.None));

        Assert.Contains("Invalid notification type", ex.Message, StringComparison.OrdinalIgnoreCase);
        repo.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Notifications.Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SavePreferenceAsync_Throws_BusinessRuleException_For_Invalid_Type()
    {
        var repo = new Mock<INotificationRepository>();
        var service = BuildService(repo);

        var dto = new UpdateNotificationPreferenceDto
        {
            NotificationType = -1,
            EmailEnabled = true,
            PushEnabled = true,
            SmsEnabled = false
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.SavePreferenceAsync(Guid.NewGuid(), dto, CancellationToken.None));

        Assert.Contains("Invalid notification type", ex.Message, StringComparison.OrdinalIgnoreCase);
        repo.Verify(r => r.GetPreferenceAsync(It.IsAny<Guid>(), It.IsAny<Domain.Enums.Notifications.NotificationType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Resolves_Email_From_User_When_Dto_Email_Is_Missing()
    {
        var repo = new Mock<INotificationRepository>();
        var userRepo = new Mock<IUserRepository>();
        var email = new Mock<IEmailService>();
        var sms = new Mock<ISmsService>();
        var push = new Mock<IPushService>();
        var realtime = new Mock<IRealtimeUpdateService>();
        var logger = new Mock<ILogger<NotificationService>>();

        realtime.Setup(x => x.PublishToUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userId = Guid.NewGuid();
        userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User
        {
            Id = userId,
            Email = "customer@example.com",
            Role = UserRole.User
        });

        repo.Setup(r => r.GetPreferenceAsync(userId, It.IsAny<Domain.Enums.Notifications.NotificationType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Notifications.NotificationPreference
            {
                UserId = userId,
                NotificationType = Domain.Enums.Notifications.NotificationType.CustomerReviewSubmitted,
                EmailEnabled = true,
                PushEnabled = false,
                SmsEnabled = false
            });

        var service = new NotificationService(repo.Object, userRepo.Object, email.Object, sms.Object, push.Object, realtime.Object, logger.Object);
        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Title = "Review submitted",
            Message = "Your review was submitted successfully.",
            Type = (int)Domain.Enums.Notifications.NotificationType.CustomerReviewSubmitted
        };

        await service.CreateAsync(dto, CancellationToken.None);

        email.Verify(x => x.SendEmailAsync(
            "customer@example.com",
            "Review submitted",
            "Your review was submitted successfully."), Times.Once);
    }
}
