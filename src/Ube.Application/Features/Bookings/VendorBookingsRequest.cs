using Ube.Application.Common.Models;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings;

public class BookingsRequest : QueryOptions
{
    public BookingStatus? Status { get; set; }
    public BookingSortBy? SortOptions { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}