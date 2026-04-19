using Ube.Domain.Enums;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Availability.Strategies;

public class CapacityStrategy : IAvailabilityStrategy
{
    public AvailabilityType Type => AvailabilityType.Capacity;

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
        else if (bookedCount >= capacity)
            status = AvailabilityStatus.Full;
        else
            status = AvailabilityStatus.Available;

        return new CalanderDayDto
        {
            Date = date,
            Status = status,
            AvailableCount = Math.Max(0, capacity - bookedCount),
            BookingCount = bookedCount,
            IsBlocked = isBlocked
        };
    }
}