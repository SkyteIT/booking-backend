using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Availability.rules;

public static class AvailabilityBlockingRules
{
   public static Result CanBlockDates(List<Booking> bookings, List<DateTime> dates)
    {
        
        if (dates == null || !dates.Any())
            return Result.Failure("No dates provided.");

        var today = DateTime.UtcNow.Date;

        var normalizedDates = dates.Select(d => d.Date).Distinct().ToList();

        var minDate = normalizedDates.Min();
        var maxDate = normalizedDates.Max();

        //Filter relevant bookings only
        var relevantBookings = bookings
            .Where(b => b.Status == BookingStatus.Confirmed &&
                        b.StartDateTime.Date <= maxDate &&
                        b.EndDateTime.Date >= minDate)
            .ToList();

        //Build HashSet of booked dates
        var bookedDates = new HashSet<DateTime>();

        foreach (var booking in relevantBookings)
        {
            var start = booking.StartDateTime.Date;
            var end = booking.EndDateTime.Date;

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                bookedDates.Add(d);
            }
        }

        // Validate dates
        var errors = new List<string>();

        foreach (var date in normalizedDates)
        {
            if (date < today)
            {
                errors.Add($"Cannot block past date {date:yyyy-MM-dd}.");
                continue;
            }

            if (bookedDates.Contains(date))
            {
                errors.Add($"Cannot block {date:yyyy-MM-dd} because it is already booked.");
            }
        }

        return errors.Any()
            ? Result.Failure(errors)
            : Result.Success();
    }
}