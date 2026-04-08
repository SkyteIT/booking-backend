using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings;

public class BookingsRequest : PagedRequest
{
    public BookingStatus? Status { get; set; }
    public BookingSortBy SortBy { get; set; }
}