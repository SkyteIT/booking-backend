using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Bookings;
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
        BookingService Service);

    private static Ctx Build()
    {
        var repo = new Mock<IBookingRepository>();
        var uow  = new Mock<IUnitOfWork>();
        return new Ctx(repo, uow, new BookingService(repo.Object, uow.Object));
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
}
