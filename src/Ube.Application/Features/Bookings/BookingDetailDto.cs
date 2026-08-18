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
    public bool CanCancel { get; set; }
    public bool CanReview { get; set; }

    // Populated when this booking already has a review, so the customer
    // sees their own review (and any vendor reply) right here instead of
    // having to navigate to My Reviews to find it.
    public Guid? ReviewId { get; set; }
    public int? ReviewRating { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? ReviewCreatedAt { get; set; }
    public string? ReviewVendorReply { get; set; }
    public DateTime? ReviewVendorReplyAt { get; set; }
}