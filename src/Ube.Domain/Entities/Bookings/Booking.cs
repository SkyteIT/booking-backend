using Ube.Domain.Enums;
namespace Ube.Domain.Entities;

public class Booking
{
       public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = default!;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = default!;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "LKR";

    public DateTime CreatedAt { get; set; }
}