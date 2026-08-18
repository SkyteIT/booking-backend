using Moq;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Fraud;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Listings;
using Ube.Domain.Enums.Payments;

namespace Ube.Tests.Bookings;

public class CheckoutServiceTests
{
    private sealed record Ctx(
        Mock<IBookingRepository> BookingRepo,
        Mock<IListingRepository> ListingRepo,
        Mock<IListingUnitRepository> UnitRepo,
        Mock<IBlockedDateRepository> BlockedDateRepo,
        Mock<ICategoryRepository> CategoryRepo,
        Mock<IPaymentService> PaymentService,
        Mock<IFraudDetectionService> FraudDetectionService,
        Mock<IUnitOfWork> UnitOfWork,
        CheckoutService Service);

    private static Ctx Build()
    {
        var bookingRepo = new Mock<IBookingRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var unitRepo = new Mock<IListingUnitRepository>();
        var blockedDateRepo = new Mock<IBlockedDateRepository>();
        var categoryRepo = new Mock<ICategoryRepository>();
        var paymentService = new Mock<IPaymentService>();
        var fraudDetectionService = new Mock<IFraudDetectionService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        // A single always-available Capacity strategy - EnsureAvailableAsync
        // is not what these tests are about, so it never blocks checkout.
        var strategy = new Mock<IAvailabilityStrategy>();
        strategy.Setup(s => s.Type).Returns(AvailabilityType.Capacity);
        strategy.Setup(s => s.CalculateAvailability(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(new CalanderDayDto { Status = AvailabilityStatus.Available });
        var strategySelector = new StrategySelector(new[] { strategy.Object });

        blockedDateRepo
            .Setup(r => r.GetByListingAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<BlockedDate>());
        bookingRepo
            .Setup(r => r.GetBookingsByListingAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Domain.Entities.Bookings.Booking>());
        bookingRepo
            .Setup(r => r.HasOverlappingBookingForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        bookingRepo.Setup(r => r.GetNextBookingSequenceAsync()).ReturnsAsync(1);
        bookingRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Domain.Entities.Bookings.Booking
            {
                Id = id,
                BookingNumber = "BKG-000001",
                Listing = new Listing { Title = "Test Listing" },
                Customer = new Domain.Entities.Users.User { FirstName = "A", LastName = "B", Email = "a@b.com" }
            });

        fraudDetectionService
            .Setup(f => f.IsNewAccountHighValueAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        paymentService
            .Setup(p => p.InitiateAsync(It.IsAny<Guid>(), It.IsAny<InitiatePaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentDto { Status = PaymentStatus.Captured });

        var service = new CheckoutService(
            bookingRepo.Object,
            listingRepo.Object,
            unitRepo.Object,
            blockedDateRepo.Object,
            categoryRepo.Object,
            strategySelector,
            paymentService.Object,
            fraudDetectionService.Object,
            unitOfWork.Object);

        return new Ctx(bookingRepo, listingRepo, unitRepo, blockedDateRepo, categoryRepo, paymentService, fraudDetectionService, unitOfWork, service);
    }

    private static Listing MakeListing(Guid categoryId) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Test Listing",
        Price = 1000m,
        Currency = "LKR",
        IsActive = true,
        AvailabilityType = AvailabilityType.Capacity,
        Capacity = 10,
        CategoryId = categoryId
    };

    private static Domain.Entities.Listings.Category MakeCategory(
        BookingConfirmationType bookingType = BookingConfirmationType.Request,
        ServiceCollectionModel collectionModel = ServiceCollectionModel.Prepay) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Category",
        BookingType = bookingType,
        PaymentCollectionModel = collectionModel,
        ServiceModel = PricingUnit.FixedPrice
    };

    private static CheckoutRequest MakeRequest(Guid listingId) => new()
    {
        IdempotencyKey = "idem-1",
        Items = new List<CheckoutItemRequest>
        {
            new()
            {
                ListingId = listingId,
                Quantity = 1,
                StartDateTime = new DateTime(2026, 9, 1),
                EndDateTime = new DateTime(2026, 9, 2)
            }
        }
    };

    [Fact]
    public async Task CheckoutAsync_Throws_And_Never_Charges_When_Customer_Already_Has_Overlapping_Booking()
    {
        var ctx = Build();
        var category = MakeCategory();
        var listing = MakeListing(category.Id);

        ctx.ListingRepo.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        ctx.CategoryRepo.Setup(r => r.GetByIdAsync(category.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        ctx.BookingRepo
            .Setup(r => r.HasOverlappingBookingForCustomerAsync(It.IsAny<Guid>(), listing.Id, null, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CheckoutAsync(Guid.NewGuid(), MakeRequest(listing.Id)));

        ctx.PaymentService.Verify(p => p.InitiateAsync(It.IsAny<Guid>(), It.IsAny<InitiatePaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        ctx.UnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        ctx.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_Charges_Normally_When_No_Duplicate_And_Not_Held()
    {
        var ctx = Build();
        var category = MakeCategory(BookingConfirmationType.Instant, ServiceCollectionModel.Prepay);
        var listing = MakeListing(category.Id);

        ctx.ListingRepo.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        ctx.CategoryRepo.Setup(r => r.GetByIdAsync(category.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await ctx.Service.CheckoutAsync(Guid.NewGuid(), MakeRequest(listing.Id));

        Assert.Single(result.Payments);
        ctx.PaymentService.Verify(p => p.InitiateAsync(It.IsAny<Guid>(), It.IsAny<InitiatePaymentRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        ctx.FraudDetectionService.Verify(f => f.CreateHoldFlagAsync(It.IsAny<Domain.Entities.Bookings.Booking>(), It.IsAny<BookingStatus>(), It.IsAny<PaymentCollectionMethod>(), It.IsAny<CancellationToken>()), Times.Never);
        ctx.FraudDetectionService.Verify(f => f.EvaluateFlagOnlyRulesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        ctx.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_Holds_Booking_And_Skips_Payment_When_New_Account_High_Value()
    {
        var ctx = Build();
        var category = MakeCategory(BookingConfirmationType.Instant, ServiceCollectionModel.Prepay);
        var listing = MakeListing(category.Id);

        ctx.ListingRepo.Setup(r => r.GetByIdAsync(listing.Id)).ReturnsAsync(listing);
        ctx.CategoryRepo.Setup(r => r.GetByIdAsync(category.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        ctx.FraudDetectionService
            .Setup(f => f.IsNewAccountHighValueAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Domain.Entities.Bookings.Booking? addedBooking = null;
        ctx.BookingRepo
            .Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Bookings.Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Bookings.Booking, CancellationToken>((b, _) => addedBooking = b)
            .Returns(Task.CompletedTask);

        var result = await ctx.Service.CheckoutAsync(Guid.NewGuid(), MakeRequest(listing.Id));

        Assert.Empty(result.Payments);
        Assert.NotNull(addedBooking);
        Assert.Equal(BookingStatus.Pending, addedBooking!.Status);
        Assert.True(addedBooking.IsHeldForFraudReview);

        ctx.PaymentService.Verify(p => p.InitiateAsync(It.IsAny<Guid>(), It.IsAny<InitiatePaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        // Instant/Prepay would have resolved to Confirmed/PlatformCollected had it not been held - that's what gets stored for later resolution.
        ctx.FraudDetectionService.Verify(f => f.CreateHoldFlagAsync(
            It.Is<Domain.Entities.Bookings.Booking>(b => b.Id == addedBooking.Id),
            BookingStatus.Confirmed,
            PaymentCollectionMethod.PlatformCollected,
            It.IsAny<CancellationToken>()), Times.Once);
        ctx.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }
}
