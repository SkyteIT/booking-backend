using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Reviews;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;

namespace Ube.Tests.Bookings;

public class BookingServiceTests
{
    private sealed record Ctx(
        Mock<IBookingRepository> BookingRepo,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IReviewRepository> ReviewRepo,
        BookingService Service);

    private static Ctx Build()
    {
        var repo = new Mock<IBookingRepository>();
        var uow  = new Mock<IUnitOfWork>();
        var reviewRepo = new Mock<IReviewRepository>();
        return new Ctx(repo, uow, reviewRepo, new BookingService(repo.Object, uow.Object, reviewRepo.Object));
    }

    private static Booking MakeBooking(Guid bookingId, Guid vendorUserId, Guid customerId, BookingStatus status)
    {
        return new Booking
        {
            Id = bookingId,
            BookingNumber = "BKG-000001",
            CustomerId = customerId,
            ListingId = Guid.NewGuid(),
            Status = status,
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(2),
            TotalAmount = 500,
            Currency = "LKR",
            CreatedAt = DateTime.UtcNow,
            Listing = new Listing
            {
                Title = "Photography Session",
                VendorProfile = new VendorProfile { UserId = vendorUserId }
            },
            Customer = new User
            {
                Id = customerId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com"
            }
        };
    }

    // --- UpdateVendorBookingStatusAsync ---

    [Fact]
    public async Task UpdateVendorBookingStatus_Throws_NotFoundException_When_Booking_Missing()
    {
        var ctx = Build();
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ctx.Service.UpdateVendorBookingStatusAsync(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Confirmed));
    }

    [Fact]
    public async Task UpdateVendorBookingStatus_Throws_BusinessRuleException_When_Wrong_Vendor()
    {
        var ctx = Build();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.UpdateVendorBookingStatusAsync(bookingId, Guid.NewGuid(), BookingStatus.Confirmed));
    }

    [Theory]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Completed)]
    public async Task UpdateVendorBookingStatus_Throws_BusinessRuleException_When_Invalid_Transition(BookingStatus alreadySet)
    {
        var ctx = Build();
        var vendorId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, vendorId, Guid.NewGuid(), alreadySet);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.UpdateVendorBookingStatusAsync(bookingId, vendorId, BookingStatus.Confirmed));
    }

    [Fact]
    public async Task UpdateVendorBookingStatus_Throws_BusinessRuleException_For_Invalid_Status()
    {
        var ctx = Build();
        var vendorId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, vendorId, Guid.NewGuid(), BookingStatus.Pending);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        // Vendor trying to set Cancelled — not an allowed vendor transition
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.UpdateVendorBookingStatusAsync(bookingId, vendorId, BookingStatus.Cancelled));
    }

    [Theory]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Rejected)]
    public async Task UpdateVendorBookingStatus_Succeeds_Sets_UpdatedAt_And_Uses_Transaction(BookingStatus newStatus)
    {
        var ctx = Build();
        var vendorId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, vendorId, Guid.NewGuid(), BookingStatus.Pending);

        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        ctx.UnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        ctx.UnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        var result = await ctx.Service.UpdateVendorBookingStatusAsync(bookingId, vendorId, newStatus);

        Assert.Equal(newStatus, result.Status);
        Assert.NotNull(booking.UpdatedAt);
        ctx.UnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
        ctx.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        ctx.BookingRepo.Verify(r => r.UpdateAsync(booking), Times.Once);
    }

    [Fact]
    public async Task UpdateVendorBookingStatus_Rolls_Back_On_Repository_Failure()
    {
        var ctx = Build();
        var vendorId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, vendorId, Guid.NewGuid(), BookingStatus.Pending);

        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        ctx.UnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        ctx.UnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        ctx.BookingRepo.Setup(r => r.UpdateAsync(It.IsAny<Booking>())).ThrowsAsync(new Exception("DB error"));

        await Assert.ThrowsAsync<Exception>(() =>
            ctx.Service.UpdateVendorBookingStatusAsync(bookingId, vendorId, BookingStatus.Confirmed));

        ctx.UnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        ctx.UnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    // --- GetBookingDetailAsync ---

    [Fact]
    public async Task GetBookingDetail_Throws_NotFoundException_When_Not_Found()
    {
        var ctx = Build();
        ctx.BookingRepo.Setup(r => r.GetBookingAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ctx.Service.GetBookingDetailAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetBookingDetail_Returns_Correct_Dto()
    {
        var ctx = Build();
        var vendorId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, vendorId, Guid.NewGuid(), BookingStatus.Pending);

        ctx.BookingRepo.Setup(r => r.GetBookingAsync(bookingId, vendorId)).ReturnsAsync(booking);

        var result = await ctx.Service.GetBookingDetailAsync(bookingId, vendorId);

        Assert.Equal(bookingId, result.BookingId);
        Assert.Equal("BKG-000001", result.BookingNumber);
        Assert.Equal("Photography Session", result.ListingTitle);
        Assert.Equal("Jane Smith", result.CustomerName);
        Assert.Equal("jane@example.com", result.CustomerEmail);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.True(result.CanConfirm);
        Assert.True(result.CanReject);
    }

    // --- GetVendorBookingsAsync ---

    [Fact]
    public async Task GetVendorBookings_Returns_Paged_Results()
    {
        var ctx = Build();
        var vendorId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var request = new BookingsRequest { PageNumber = 1, PageSize = 10 };

        var bookings = new List<Booking>
        {
            MakeBooking(Guid.NewGuid(), vendorId, customerId, BookingStatus.Pending),
            MakeBooking(Guid.NewGuid(), vendorId, customerId, BookingStatus.Confirmed)
        };

        ctx.BookingRepo
            .Setup(r => r.GetBookingsByVendorIdAsync(vendorId, request))
            .ReturnsAsync(new PagedResult<Booking>
            {
                Items = bookings,
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2,
                TotalPages = 1
            });

        var result = await ctx.Service.GetVendorBookingsAsync(vendorId, request);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
    }

    // --- CancelBookingAsync ---

    [Fact]
    public async Task CancelBooking_Throws_NotFoundException_When_Booking_Missing()
    {
        var ctx = Build();
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ctx.Service.CancelBookingAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task CancelBooking_Throws_BusinessRuleException_When_Not_Owner()
    {
        var ctx = Build();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CancelBookingAsync(bookingId, Guid.NewGuid()));
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    public async Task CancelBooking_Succeeds_For_Pending_Or_Confirmed_Owned_Booking(BookingStatus status)
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, Guid.NewGuid(), customerId, status);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        var result = await ctx.Service.CancelBookingAsync(bookingId, customerId);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(BookingStatus.Cancelled, result.Status);
        ctx.BookingRepo.Verify(r => r.UpdateAsync(booking), Times.Once);
    }

    [Theory]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Rejected)]
    public async Task CancelBooking_Throws_BusinessRuleException_For_Non_Cancellable_Status(BookingStatus status)
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, Guid.NewGuid(), customerId, status);
        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.CancelBookingAsync(bookingId, customerId));
    }

    // --- GetCustomerBookingsAsync ---

    [Fact]
    public async Task GetCustomerBookings_Maps_Paged_Result()
    {
        var ctx = Build();
        var vendorId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var request = new BookingsRequest { PageNumber = 1, PageSize = 10 };

        var bookings = new List<Booking>
        {
            MakeBooking(Guid.NewGuid(), vendorId, customerId, BookingStatus.Pending)
        };

        ctx.BookingRepo
            .Setup(r => r.GetBookingsByCustomerIdAsync(customerId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Booking>
            {
                Items = bookings,
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1,
                TotalPages = 1
            });

        var result = await ctx.Service.GetCustomerBookingsAsync(customerId, request);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    // --- GetCustomerBookingDetailAsync (CanReview) ---

    [Fact]
    public async Task GetCustomerBookingDetail_CanReview_True_For_Completed_Booking_Without_Review()
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, Guid.NewGuid(), customerId, BookingStatus.Completed);

        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        ctx.ReviewRepo.Setup(r => r.ExistsByBookingIdAsync(bookingId)).ReturnsAsync(false);

        var result = await ctx.Service.GetCustomerBookingDetailAsync(bookingId, customerId);

        Assert.True(result.CanReview);
    }

    [Fact]
    public async Task GetCustomerBookingDetail_CanReview_False_When_Review_Already_Exists()
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, Guid.NewGuid(), customerId, BookingStatus.Completed);

        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        ctx.ReviewRepo.Setup(r => r.ExistsByBookingIdAsync(bookingId)).ReturnsAsync(true);

        var result = await ctx.Service.GetCustomerBookingDetailAsync(bookingId, customerId);

        Assert.False(result.CanReview);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Rejected)]
    public async Task GetCustomerBookingDetail_CanReview_False_When_Not_Completed(BookingStatus status)
    {
        var ctx = Build();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = MakeBooking(bookingId, Guid.NewGuid(), customerId, status);

        ctx.BookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        var result = await ctx.Service.GetCustomerBookingDetailAsync(bookingId, customerId);

        Assert.False(result.CanReview);
        ctx.ReviewRepo.Verify(r => r.ExistsByBookingIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    // --- CompleteExpiredBookingsAsync ---

    [Fact]
    public async Task CompleteExpiredBookings_Completes_Confirmed_Booking_Past_End_Date()
    {
        var ctx = Build();
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Confirmed);
        booking.EndDateTime = DateTime.UtcNow.AddDays(-1);

        ctx.BookingRepo
            .Setup(r => r.GetBookingsPastEndDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { booking });
        ctx.UnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        ctx.UnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

        var completedCount = await ctx.Service.CompleteExpiredBookingsAsync();

        Assert.Equal(1, completedCount);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.NotNull(booking.UpdatedAt);
        ctx.BookingRepo.Verify(r => r.UpdateAsync(booking), Times.Once);
        ctx.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Rejected)]
    [InlineData(BookingStatus.Completed)]
    public async Task CompleteExpiredBookings_Skips_Non_Confirmed_Bookings(BookingStatus status)
    {
        var ctx = Build();
        // Defensive test: even if the repo query somehow returned a
        // non-Confirmed booking, CanSystemComplete must still reject it -
        // the sweep never assigns status without going through the state
        // machine.
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), status);
        booking.EndDateTime = DateTime.UtcNow.AddDays(-1);

        ctx.BookingRepo
            .Setup(r => r.GetBookingsPastEndDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { booking });

        var completedCount = await ctx.Service.CompleteExpiredBookingsAsync();

        Assert.Equal(0, completedCount);
        Assert.Equal(status, booking.Status);
        ctx.BookingRepo.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CompleteExpiredBookings_Isolates_Failure_And_Still_Completes_Others()
    {
        var ctx = Build();
        var failing = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Confirmed);
        failing.EndDateTime = DateTime.UtcNow.AddDays(-2);
        var succeeding = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Confirmed);
        succeeding.EndDateTime = DateTime.UtcNow.AddDays(-1);

        ctx.BookingRepo
            .Setup(r => r.GetBookingsPastEndDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking> { failing, succeeding });
        ctx.UnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        ctx.UnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        ctx.UnitOfWork.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        ctx.BookingRepo.Setup(r => r.UpdateAsync(failing)).ThrowsAsync(new Exception("DB error"));

        var completedCount = await ctx.Service.CompleteExpiredBookingsAsync();

        // Only the succeeding booking counts - the failing one's update
        // was attempted and rolled back, but didn't abort the loop.
        Assert.Equal(1, completedCount);
        Assert.Equal(BookingStatus.Completed, succeeding.Status);
        ctx.BookingRepo.Verify(r => r.UpdateAsync(failing), Times.Once);
        ctx.UnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        ctx.UnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }
}
