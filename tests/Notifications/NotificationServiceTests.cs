using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.DTOs.Notification;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Interfaces;
using Ube.Application.Services;
using Microsoft.Extensions.Logging;

namespace Ube.Tests.Notifications;

public class NotificationServiceTests
{
    private static NotificationService BuildService(Mock<INotificationRepository> repo)
    {
        var email = new Mock<IEmailService>();
        var sms = new Mock<ISmsService>();
        var realtime = new Mock<IRealtimeUpdateService>();
        var logger = new Mock<ILogger<NotificationService>>();
        realtime.Setup(x => x.PublishToUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new NotificationService(repo.Object, email.Object, sms.Object, realtime.Object, logger.Object);
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
}
