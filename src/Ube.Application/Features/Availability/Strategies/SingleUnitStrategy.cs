using Ube.Domain.Enums;
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
        AvailabilityStatus status;

        if (capacity <= 0)
            status = AvailabilityStatus.Full;
        else if (isBlocked)
            status = AvailabilityStatus.Blocked;
        else if (bookedCount > 0)
            status = AvailabilityStatus.Full;
        else
            status = AvailabilityStatus.Available;

        return new CalanderDayDto
        {
            Date = date,
            Status = status,
            AvailableCount = bookedCount > 0 ? 0 : 1,
            BookingCount = bookedCount,
            IsBlocked = isBlocked
        };
    }
}