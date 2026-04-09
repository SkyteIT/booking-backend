

using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Availablity.Strategies;

public class CapacityStrategy : IAvailabilityStrategy
{
    public CalanderDayDto CalculateAvailability(
        DateTime date,
        int capacity,
        int bookedCount,
        bool isBlocked
    )
    {
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
