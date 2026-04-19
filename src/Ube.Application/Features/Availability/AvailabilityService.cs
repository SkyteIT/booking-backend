using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Availability.rules;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Common.Exceptions;


using Ube.Domain.Entities.Listings;

namespace Ube.Application.Features.Availability;

public class AvailabilityService : IAvailabilityService
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

    public async Task<List<CalanderDayDto>> GetCalanderAsync(Guid listingId, Guid vendorId, int month, int year)
    {
        // Get listing(use this for get availability type)
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null) throw new KeyNotFoundException("Listing not found");
        var authResult = AvailabilityAuthorizationRules.CanModifyAvailability(listing, vendorId);

        if (!authResult.IsSuccess)
        {
            throw new ForbiddenException(authResult.ErrorMessage);
        }
        var today = DateTime.UtcNow.Date;
        if (year < today.Year || (year == today.Year && month < today.Month))
        {
            throw new BusinessRuleException("Cannot get calendar for past months");
        }


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

        var bookingMap = new Dictionary<DateTime, int>();
        foreach (var booking in bookings)
        {
            for (var d = booking.StartDateTime.Date; d <= booking.EndDateTime.Date; d = d.AddDays(1))
            {
                if (bookingMap.ContainsKey(d))
                {
                    bookingMap[d] += 1;
                }
                else
                {
                    bookingMap[d] = 1;
                }
            }
        }
        for ( var date = startDate ; date <= endDate; date = date.AddDays(1))
        {
            var isBlocked = blockSet.Contains(date.Date);
            
            // Count bookings for the day (check if booking overlaps with the day)
            var bookingCount = bookingMap.TryGetValue(date.Date, out var count) ? count : 0;
            // Calculate availability for the day using strategy
            var day = strategy.CalculateAvailability(
                date.Date,
                listing.Capacity,
                bookingCount,
                isBlocked
            );
            result.Add(day);

        }
        return result;
    }
    public async Task BlockdatesAsync(Guid listingId, Guid vendorId, List<DateTime> dates)
    { 
        
        
        // Normalize dates to date only and remove duplicates
        var normalizeDates = dates
            .Select(d => d.Date)
            .Distinct()
            .ToList();

        var today = DateTime.UtcNow.Date;
        if (normalizeDates.Any(d => d <= today))
        {
            throw new BusinessRuleException("Cannot block past dates");
        }
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null) throw new KeyNotFoundException("Listing not found");

        var authResult = AvailabilityAuthorizationRules
            .CanModifyAvailability(listing, vendorId);

        if (!authResult.IsSuccess)
            throw new ForbiddenException(authResult.ErrorMessage);
        
        var bookings = await _bookingRepository
            .GetBookingsByListingAndDateRangeAsync(
                listingId,
                normalizeDates.Min(),
                normalizeDates.Max()
            );

        var ruleResult = AvailabilityBlockingRules.CanBlockDates(bookings, normalizeDates);
        if (!ruleResult.IsSuccess)
        {
            throw new BusinessRuleException(ruleResult.ErrorMessage);
        }
        // Check if any of the dates are already blocked
        var existing = await _blockedDateRepository
            .GetByListingAndDatesAsync(listingId, normalizeDates);
        var exsistingDates = existing
            .Select( x => x.Date)
            .ToHashSet();
        var alreadyBlocked = normalizeDates
            .Where(d => exsistingDates.Contains(d))
            .ToList();

        if (alreadyBlocked.Any())
        {
            throw new BusinessRuleException(
                $"These dates are already blocked: {string.Join(", ", alreadyBlocked.Select(d => d.ToString("yyyy-MM-dd")))}"
            );
        }
        var newDates = normalizeDates
            .Where(d => !exsistingDates.Contains(d))
            .ToList();

        var blockedDates = newDates
            .Select(d => new BlockedDate
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                Date = d
            }).ToList();

        if(blockedDates.Any())
        {
            await _blockedDateRepository.AddRangeAsync(blockedDates);
        }
    }
    public async Task UnBlockdatesAsync(Guid listingId, Guid vendorId, List<DateTime> dates)
    {
        if (dates ==  null || !dates.Any()){
            throw new BusinessRuleException("Dates are required");
        }
        var today = DateTime.UtcNow.Date;

        if (dates.Any(d => d.Date <= today))
        {
            throw new BusinessRuleException("Cannot unblock past dates");
        }
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null) throw new KeyNotFoundException("Listing not found");

        var authResult = AvailabilityAuthorizationRules
            .CanModifyAvailability(listing, vendorId);

        if (!authResult.IsSuccess)
            throw new ForbiddenException(authResult.ErrorMessage);

        // Normalize dates to date only and remove duplicates
         var normalizeDates = dates
            .Select(d => d.Date)
            .Distinct()
            .ToList();
        // Get existing blocked dates for the listing and specified dates
        var existing = await _blockedDateRepository
            .GetByListingAndDatesAsync(listingId, normalizeDates);
        var existingDates = existing.Select(x => x.Date).ToHashSet();

        var missingDates = normalizeDates
            .Where(d => !existingDates.Contains(d))
            .ToList();

        if (missingDates.Any())
        {
            throw new BusinessRuleException(
                $"These dates are not blocked: {string.Join(", ", missingDates.Select(d => d.ToString("yyyy-MM-dd")))}"
            );
        }
        if(existing.Any())
        {
            await _blockedDateRepository.RemoveRangeAsync(existing);
        }
    }
}
