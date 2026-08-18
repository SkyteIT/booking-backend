using Ube.Domain.Enums.Listings;
namespace Ube.Application.Features.Availability;

public interface IAvailabilityStrategy
{
    // Tell which type this strategy support
    AvailabilityType Type { get; }

    // Calculate availability for a one date 
    CalanderDayDto CalculateAvailability(
        DateTime date,
        int capacity, 
        int bookedCount, 
        bool isBlocked
    );
}