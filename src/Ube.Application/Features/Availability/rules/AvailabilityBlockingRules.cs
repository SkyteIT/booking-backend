using Ube.Domain.Entities.Bookings;

namespace Ube.Application.Features.Availability.rules;

public static class AvailabilityBlockingRules
{
   public static Result CanBlockDates(List<Booking> bookings, List<DateTime> dates)
    {
        var errors = new List<string>();
        var bookedDates =  new HashSet<DateTime>();

        foreach (var booking in bookings)
        {
            for (var d = booking.StartDateTime.Date;
             d <= booking.EndDateTime.Date; 
             d = d.AddDays(1))
            {
                bookedDates.Add(d);
            }
        }


        foreach (var date in dates.Select(d => d.Date))
        {
            if (bookedDates.Contains(date))
            {
                errors.Add($"Date {date:yyyy-MM-dd} already has a booking.");
            }
        }

        if (errors.Any())
            return Result.Failure(errors);

        return Result.Success();
    } 
}