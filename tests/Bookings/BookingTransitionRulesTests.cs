using Ube.Application.Features.Bookings;
using Ube.Domain.Enums.Bookings;

namespace Ube.Tests.Bookings;

public class BookingTransitionRulesTests
{
    // --- CanTransition (general) ---

    [Theory]
    [InlineData(BookingStatus.Pending,   BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Pending,   BookingStatus.Rejected)]
    [InlineData(BookingStatus.Pending,   BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Completed)]
    public void CanTransition_Returns_True_For_Valid_Transitions(BookingStatus from, BookingStatus to)
    {
        Assert.True(BookingTransitionRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData(BookingStatus.Confirmed,  BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed,  BookingStatus.Rejected)]
    [InlineData(BookingStatus.Rejected,   BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Cancelled,  BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Completed,  BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Completed,  BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Pending,    BookingStatus.Completed)]
    public void CanTransition_Returns_False_For_Invalid_Transitions(BookingStatus from, BookingStatus to)
    {
        Assert.False(BookingTransitionRules.CanTransition(from, to));
    }

    // --- CanVendorTransition ---

    [Theory]
    [InlineData(BookingStatus.Pending, BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Pending, BookingStatus.Rejected)]
    public void CanVendorTransition_Returns_True_For_Allowed_Vendor_Actions(BookingStatus from, BookingStatus to)
    {
        Assert.True(BookingTransitionRules.CanVendorTransition(from, to));
    }

    [Theory]
    [InlineData(BookingStatus.Pending,   BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Completed)]
    public void CanVendorTransition_Returns_False_For_Disallowed_Vendor_Actions(BookingStatus from, BookingStatus to)
    {
        Assert.False(BookingTransitionRules.CanVendorTransition(from, to));
    }

    // --- CanCustomerTransition ---

    [Theory]
    [InlineData(BookingStatus.Pending,   BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Cancelled)]
    public void CanCustomerTransition_Returns_True_For_Allowed_Customer_Actions(BookingStatus from, BookingStatus to)
    {
        Assert.True(BookingTransitionRules.CanCustomerTransition(from, to));
    }

    [Theory]
    [InlineData(BookingStatus.Pending,   BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Pending,   BookingStatus.Rejected)]
    [InlineData(BookingStatus.Completed, BookingStatus.Cancelled)]
    public void CanCustomerTransition_Returns_False_For_Disallowed_Customer_Actions(BookingStatus from, BookingStatus to)
    {
        Assert.False(BookingTransitionRules.CanCustomerTransition(from, to));
    }

    // --- CanSystemTransition ---

    [Fact]
    public void CanSystemTransition_Returns_True_For_Confirmed_To_Completed()
    {
        Assert.True(BookingTransitionRules.CanSystemTransition(BookingStatus.Confirmed, BookingStatus.Completed));
    }

    [Theory]
    [InlineData(BookingStatus.Pending,   BookingStatus.Completed)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Rejected,  BookingStatus.Completed)]
    public void CanSystemTransition_Returns_False_For_Invalid_System_Actions(BookingStatus from, BookingStatus to)
    {
        Assert.False(BookingTransitionRules.CanSystemTransition(from, to));
    }
}
