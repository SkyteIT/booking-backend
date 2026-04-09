using Ube.Domain.Enums.Listings;
namespace Ube.Application.Features.Availablity;

public interface IAvailabilityStrategy
{
    CalanderDayDto CalculateAvailability(
        DateTime date,
        int capacity, 
        int bookedCount, 
        bool isBlocked
    );
}