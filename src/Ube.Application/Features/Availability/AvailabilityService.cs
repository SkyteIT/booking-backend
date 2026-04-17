using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Availability.Strategies;
using Ube.Domain.Entities.Bookings;

namespace Ube.Application.Features.Availability;

public class AvailabilityService
{
    private readonly StrategySelector _strategySelector;
    private readonly IListingRepository _listingRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IBlockedDateRepository _blockedDateRepository;

    public AvailabilityService(
        IBlockedDateRepository blockedDateRepository,
        StrategySelector strategySelector,
        IListingRepository listingRepository,
        IBookingRepository bookingRepository)
    {
        _strategySelector = strategySelector;
        _bookingRepository = bookingRepository;
        _listingRepository = listingRepository;
        _blockedDateRepository = blockedDateRepository;
    }

    public async Task<List<CalanderDayDto>> GetCalanderAsync(Guid listingId, int month, int year)
    {
        //Vailidate  month 
        if(month <1 || month > 12) throw new ArgumentException ("Invalid month");

        // Get listing(use this for get availability type)
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null) throw new ArgumentException("Listing not found");

        // Get strategy using listing availability type
        var strategy = _strategySelector.GetStrategy(listing.AvailabilityType);

        // calculate Date range of month
        var startDate = new DateTime(year, month,1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        // Get blocked dates for listing in date range 
        var blockedDates = await _blockedDateRepository.GetByListingAndDateRangeAsync(listingId, startDate, endDate);
        //use hashset for faster lookup
        var blockSet = blockedDates
            .Select ( bd => bd.Date.Date)
            .ToHashSet();
        var bookings = await _bookingRepository.GetBookingsByListingAndDateRangeAsync(listingId, startDate, endDate);

        var result = new List<CalanderDayDto>();
        for ( var date = startDate ; date <= endDate; date = date.AddDays(1))
        {
            var isBlocked = blockSet.Contains(date.Date);
            
            // Count bookings for the day (check if booking overlaps with the day)
            var bookingCount = bookings.Count(
                b => b.StartDateTime.Date <= date.Date && b.EndDateTime.Date >= date.Date
            );
            // Calculate availability for the day using strategy
            var day = strategy.CalculateAvailability(
                date.Date,
                bookingCount,
                listing.Capacity,
                isBlocked
            );
            result.Add(day);

        }
        return result;
    }

}
