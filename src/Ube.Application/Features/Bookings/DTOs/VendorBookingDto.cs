
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings.DTOs;

public class VendorBookingDto
{
    public string BookingNumber { get; set;} = string.Empty;
    public string ListingTitle { get; set;} = string.Empty;
    public string CustomerName { get; set;} = string.Empty;
    public DateTime StartDateTime { get; set;}
    public DateTime EndDateTime { get; set;}
    public BookingStatus Status { get; set;}
    public decimal TotalAmount { get; set;}
    public string Currency { get; set;} = string.Empty;
    public DateTime CreatedAt { get; set;}
}