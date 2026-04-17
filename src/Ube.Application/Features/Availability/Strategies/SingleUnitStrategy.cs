using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Availability.Strategies;

public class SingleUnitStrategy : IAvailabilityStrategy
{
    public AvailabilityType Type => AvailabilityType.SingleUnit;
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

        if (bookedCount > 0)
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
            AvailableCount = 1
        };
    }
}