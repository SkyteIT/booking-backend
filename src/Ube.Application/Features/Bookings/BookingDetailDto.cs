using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings;
public class BookingDetailDto
{
    public Guid BookingId { get; set;}
    public string BookingNumber { get; set; } = string.Empty;
    public string ListingTitle { get; set; } = string.Empty;
    public string CustomerName { get;set; } =string.Empty;
    public string CustomerEmail { get; set;} = string.Empty;
    public DateTime StartDateTime {get; set;}
    public DateTime EndDateTime { get; set;}
    public BookingStatus Status {get; set;}
    public decimal TotalAmount { get ; set;}
    public string Currency { get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;}
    public bool CanConfirm { get; set; }
    public bool CanReject { get; set; }
}