
using Ube.Domain.Enums.Listings;
namespace Ube.Application.Features.Availability.Strategies;

public class CapacityStrategy : IAvailabilityStrategy
{
    public AvailabilityType Type => AvailabilityType.Capacity;
    // calculates availability based on the total capacity of the listing and the number of bookings.
    public CalanderDayDto CalculateAvailability(
        
        DateTime date,
        int capacity,
        int bookedCount,
        bool isBlocked
    )
    {
         if (capacity <= 0)
        {
            return new CalanderDayDto
            {
                Date = date,
                Status = AvailabilityStatus.Full,
                AvailableCount = 0
            };
        }
        if (isBlocked)
        {
            return new CalanderDayDto
            {
                Date = date,
                Status = AvailabilityStatus.Blocked,
                AvailableCount = 0
            };
        }

        var available = capacity - bookedCount;
        if (available <= 0)
        {
            return new CalanderDayDto
            {
                Date = date,
                Status = AvailabilityStatus.Full,
                AvailableCount = 0
            };
        }

        return new CalanderDayDto
        {
            Date = date,
            Status = AvailabilityStatus.Available,
            AvailableCount = available
        };
    }
}
