using Ube.Application.Features.Bookings;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Bookings;

namespace Ube.Tests.Bookings;

public class BookingValidationRulesTests
{
    private static Booking MakeBooking(Guid vendorUserId, Guid customerId, BookingStatus status)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = status,
            Listing = new Listing
            {
                VendorProfile = new VendorProfile { UserId = vendorUserId }
            },
            Customer = new User { Id = customerId, FirstName = "Test", LastName = "User", Email = "t@t.com" }
        };
    }

    // --- BelongsToVendorListing ---

    [Fact]
    public void BelongsToVendorListing_Succeeds_When_Vendor_Owns_Listing()
    {
        var vendorId = Guid.NewGuid();
        var booking = MakeBooking(vendorId, Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.BelongsToVendorListing(booking, vendorId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BelongsToVendorListing_Fails_When_Different_Vendor()
    {
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.BelongsToVendorListing(booking, Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    // --- CanVendorConfirm ---

    [Fact]
    public void CanVendorConfirm_Succeeds_For_Pending_Booking_Owner()
    {
        var vendorId = Guid.NewGuid();
        var booking = MakeBooking(vendorId, Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.CanVendorConfirm(booking, vendorId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CanVendorConfirm_Fails_When_Not_Vendor_Owner()
    {
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.CanVendorConfirm(booking, Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Rejected)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Completed)]
    public void CanVendorConfirm_Fails_When_Booking_Not_Pending(BookingStatus status)
    {
        var vendorId = Guid.NewGuid();
        var booking = MakeBooking(vendorId, Guid.NewGuid(), status);

        var result = BookingValidationRules.CanVendorConfirm(booking, vendorId);

        Assert.False(result.IsSuccess);
    }

    // --- CanVendorReject ---

    [Fact]
    public void CanVendorReject_Succeeds_For_Pending_Booking_Owner()
    {
        var vendorId = Guid.NewGuid();
        var booking = MakeBooking(vendorId, Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.CanVendorReject(booking, vendorId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CanVendorReject_Fails_When_Not_Vendor_Owner()
    {
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.CanVendorReject(booking, Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Rejected)]
    [InlineData(BookingStatus.Cancelled)]
    public void CanVendorReject_Fails_When_Booking_Not_Pending(BookingStatus status)
    {
        var vendorId = Guid.NewGuid();
        var booking = MakeBooking(vendorId, Guid.NewGuid(), status);

        var result = BookingValidationRules.CanVendorReject(booking, vendorId);

        Assert.False(result.IsSuccess);
    }

    // --- BelongsToUser ---

    [Fact]
    public void BelongsToUser_Succeeds_When_Customer_Matches()
    {
        var customerId = Guid.NewGuid();
        var booking = MakeBooking(Guid.NewGuid(), customerId, BookingStatus.Pending);

        var result = BookingValidationRules.BelongsToUser(booking, customerId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BelongsToUser_Fails_When_Different_Customer()
    {
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.BelongsToUser(booking, Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    // --- CanUserCancelPending ---

    [Fact]
    public void CanUserCancelPending_Succeeds_For_Pending_Booking_Owner()
    {
        var customerId = Guid.NewGuid();
        var booking = MakeBooking(Guid.NewGuid(), customerId, BookingStatus.Pending);

        var result = BookingValidationRules.CanUserCancelPendding(booking, customerId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CanUserCancelPending_Fails_When_Not_Owner()
    {
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending);

        var result = BookingValidationRules.CanUserCancelPendding(booking, Guid.NewGuid());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CanUserCancelPending_Fails_When_Already_Confirmed()
    {
        var customerId = Guid.NewGuid();
        var booking = MakeBooking(Guid.NewGuid(), customerId, BookingStatus.Confirmed);

        var result = BookingValidationRules.CanUserCancelPendding(booking, customerId);

        Assert.False(result.IsSuccess);
    }

    // --- CanUserCancelConfirmed ---

    [Fact]
    public void CanUserCancelConfirmed_Succeeds_For_Confirmed_Booking_Owner()
    {
        var customerId = Guid.NewGuid();
        var booking = MakeBooking(Guid.NewGuid(), customerId, BookingStatus.Confirmed);

        var result = BookingValidationRules.CanUserCancelConfirmed(booking, customerId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CanUserCancelConfirmed_Fails_When_Booking_Is_Pending()
    {
        var customerId = Guid.NewGuid();
        var booking = MakeBooking(Guid.NewGuid(), customerId, BookingStatus.Pending);

        var result = BookingValidationRules.CanUserCancelConfirmed(booking, customerId);

        Assert.False(result.IsSuccess);
    }

    // --- CanSystemComplete ---

    [Fact]
    public void CanSystemComplete_Succeeds_For_Confirmed_Booking()
    {
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Confirmed);

        var result = BookingValidationRules.CanSystemComplete(booking);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Rejected)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Completed)]
    public void CanSystemComplete_Fails_When_Booking_Not_Confirmed(BookingStatus status)
    {
        var booking = MakeBooking(Guid.NewGuid(), Guid.NewGuid(), status);

        var result = BookingValidationRules.CanSystemComplete(booking);

        Assert.False(result.IsSuccess);
    }
}
