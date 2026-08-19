using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models;
using Ube.Application.Features.Fraud;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Fraud;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Fraud;
using Ube.Domain.Enums.Payments;

namespace Ube.Tests.Fraud;

public class FraudDetectionServiceTests
{
    private sealed record Ctx(
        Mock<IFraudFlagRepository> FlagRepo,
        Mock<IBookingRepository> BookingRepo,
        Mock<IUserRepository> UserRepo,
        Mock<IPaymentService> PaymentService,
        Mock<IAdminAlertService> AdminAlertService,
        FraudDetectionOptions Options,
        FraudDetectionService Service);

    private static Ctx Build(FraudDetectionOptions? options = null)
    {
        var flagRepo = new Mock<IFraudFlagRepository>();
        var bookingRepo = new Mock<IBookingRepository>();
        var userRepo = new Mock<IUserRepository>();
        var paymentService = new Mock<IPaymentService>();
        var adminAlertService = new Mock<IAdminAlertService>();
        var opts = options ?? new FraudDetectionOptions();

        var service = new FraudDetectionService(
            flagRepo.Object,
            bookingRepo.Object,
            userRepo.Object,
            paymentService.Object,
            adminAlertService.Object,
            Options.Create(opts),
            NullLogger<FraudDetectionService>.Instance);

        return new Ctx(flagRepo, bookingRepo, userRepo, paymentService, adminAlertService, opts, service);
    }

    private static User MakeUser(DateTime createdAt) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "A",
        LastName = "B",
        Email = "a@b.com",
        CreatedAt = createdAt
    };

    [Fact]
    public async Task IsNewAccountHighValue_True_When_Account_New_And_Amount_High()
    {
        var ctx = Build();
        var user = MakeUser(DateTime.UtcNow.AddHours(-1));
        ctx.UserRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await ctx.Service.IsNewAccountHighValueAsync(user.Id, ctx.Options.HighValueThreshold + 1);

        Assert.True(result);
    }

    [Fact]
    public async Task IsNewAccountHighValue_False_When_Account_Old()
    {
        var ctx = Build();
        var user = MakeUser(DateTime.UtcNow.AddDays(-30));
        ctx.UserRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await ctx.Service.IsNewAccountHighValueAsync(user.Id, ctx.Options.HighValueThreshold + 1);

        Assert.False(result);
    }

    [Fact]
    public async Task IsNewAccountHighValue_False_When_Amount_Below_Threshold()
    {
        var ctx = Build();
        var user = MakeUser(DateTime.UtcNow.AddHours(-1));
        ctx.UserRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await ctx.Service.IsNewAccountHighValueAsync(user.Id, ctx.Options.HighValueThreshold - 1);

        Assert.False(result);
    }

    [Fact]
    public async Task IsNewAccountHighValue_False_When_Disabled()
    {
        var ctx = Build(new FraudDetectionOptions { Enabled = false });
        var result = await ctx.Service.IsNewAccountHighValueAsync(Guid.NewGuid(), 1_000_000m);

        Assert.False(result);
        ctx.UserRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateFlagOnlyRules_Flags_Velocity_When_At_Threshold()
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        ctx.BookingRepo.Setup(r => r.CountByCustomerSinceAsync(customerId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ctx.Options.MaxBookingsPerWindow);
        ctx.BookingRepo.Setup(r => r.CountCancelledByCustomerSinceAsync(customerId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        await ctx.Service.EvaluateFlagOnlyRulesAsync(customerId, bookingId);

        ctx.FlagRepo.Verify(r => r.AddAsync(
            It.Is<FraudFlag>(f => f.RuleTriggered == FraudRuleType.BookingVelocity && f.Severity == FraudFlagSeverity.FlagOnly),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateFlagOnlyRules_Flags_RepeatedCancellations_When_Over_Threshold()
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        ctx.BookingRepo.Setup(r => r.CountByCustomerSinceAsync(customerId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        ctx.BookingRepo.Setup(r => r.CountCancelledByCustomerSinceAsync(customerId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ctx.Options.MaxCancellationsPerWindow + 1);

        await ctx.Service.EvaluateFlagOnlyRulesAsync(customerId, bookingId);

        ctx.FlagRepo.Verify(r => r.AddAsync(
            It.Is<FraudFlag>(f => f.RuleTriggered == FraudRuleType.RepeatedCancellations && f.Severity == FraudFlagSeverity.FlagOnly),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateFlagOnlyRules_Flags_Nothing_When_Below_Both_Thresholds()
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        ctx.BookingRepo.Setup(r => r.CountByCustomerSinceAsync(customerId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ctx.Options.MaxBookingsPerWindow - 1);
        ctx.BookingRepo.Setup(r => r.CountCancelledByCustomerSinceAsync(customerId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ctx.Options.MaxCancellationsPerWindow);

        await ctx.Service.EvaluateFlagOnlyRulesAsync(customerId, Guid.NewGuid());

        ctx.FlagRepo.Verify(r => r.AddAsync(It.IsAny<FraudFlag>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateFlagOnlyRules_Never_Throws_When_Repository_Fails()
    {
        var ctx = Build();
        ctx.BookingRepo.Setup(r => r.CountByCustomerSinceAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var ex = await Record.ExceptionAsync(() => ctx.Service.EvaluateFlagOnlyRulesAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Null(ex);
    }

    private static FraudFlag MakeHoldFlag(Guid bookingId, Guid customerId) => new()
    {
        Id = Guid.NewGuid(),
        BookingId = bookingId,
        CustomerId = customerId,
        RuleTriggered = FraudRuleType.NewAccountHighValue,
        Severity = FraudFlagSeverity.Hold,
        Details = "test",
        Status = FraudFlagStatus.Open,
        ResolvedStatusOnClear = BookingStatus.Confirmed,
        CollectionMethodOnClear = PaymentCollectionMethod.PlatformCollected
    };

    [Fact]
    public async Task ReviewAsync_Clear_On_Hold_Resolves_Booking_And_Initiates_Payment_Once()
    {
        var ctx = Build();
        var booking = new Booking { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Status = BookingStatus.Pending, IsHeldForFraudReview = true };
        var flag = MakeHoldFlag(booking.Id, booking.CustomerId);

        ctx.FlagRepo.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(flag);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        var result = await ctx.Service.ReviewAsync(Guid.NewGuid(), flag.Id, new ReviewFraudFlagRequest { Decision = FraudReviewDecision.Clear });

        Assert.Equal(FraudFlagStatus.Cleared, result.Status);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.False(booking.IsHeldForFraudReview);
        ctx.PaymentService.Verify(p => p.InitiateAsync(booking.CustomerId, It.IsAny<InitiatePaymentRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        ctx.BookingRepo.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task ReviewAsync_ConfirmFraud_On_Hold_Rejects_Booking_And_Never_Pays()
    {
        var ctx = Build();
        var booking = new Booking { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Status = BookingStatus.Pending, IsHeldForFraudReview = true };
        var flag = MakeHoldFlag(booking.Id, booking.CustomerId);

        ctx.FlagRepo.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(flag);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        var result = await ctx.Service.ReviewAsync(Guid.NewGuid(), flag.Id, new ReviewFraudFlagRequest { Decision = FraudReviewDecision.ConfirmFraud });

        Assert.Equal(FraudFlagStatus.ConfirmedFraud, result.Status);
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.False(booking.IsHeldForFraudReview);
        ctx.PaymentService.Verify(p => p.InitiateAsync(It.IsAny<Guid>(), It.IsAny<InitiatePaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReviewAsync_On_FlagOnly_Never_Touches_Booking_Or_Payment()
    {
        var ctx = Build();
        var flag = new FraudFlag
        {
            Id = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            RuleTriggered = FraudRuleType.BookingVelocity,
            Severity = FraudFlagSeverity.FlagOnly,
            Details = "test",
            Status = FraudFlagStatus.Open
        };
        ctx.FlagRepo.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(flag);

        var result = await ctx.Service.ReviewAsync(Guid.NewGuid(), flag.Id, new ReviewFraudFlagRequest { Decision = FraudReviewDecision.Clear, Notes = "looks fine" });

        Assert.Equal(FraudFlagStatus.Cleared, result.Status);
        ctx.BookingRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        ctx.PaymentService.Verify(p => p.InitiateAsync(It.IsAny<Guid>(), It.IsAny<InitiatePaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReviewAsync_Throws_When_Flag_Already_Reviewed()
    {
        var ctx = Build();
        var flag = MakeHoldFlag(Guid.NewGuid(), Guid.NewGuid());
        flag.Status = FraudFlagStatus.Cleared;
        ctx.FlagRepo.Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(flag);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.ReviewAsync(Guid.NewGuid(), flag.Id, new ReviewFraudFlagRequest { Decision = FraudReviewDecision.Clear }));
    }
}
