using Ube.Application.Common.Interfaces.Persistence;
namespace Ube.Application.Common.Helpers;
public class RatingHelper
{
    private readonly IListingRepository _listingRepo;

    public RatingHelper(IListingRepository listingRepo)
    {
        _listingRepo = listingRepo;
    }

    // Helper to update listing rating when a review is created, updated, or deleted.
    public async Task UpdateListingRatingAsync(Guid listingId, int? oldRating, int? newRating)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId);

        if (listing == null)
            return;

        var total = listing.AverageRating * listing.TotalReviews;

        // 🔹 REMOVE OLD RATING
        if (oldRating.HasValue)
        {
            total -= oldRating.Value;
            listing.TotalReviews--;
        }

        // 🔹 ADD NEW RATING
        if (newRating.HasValue)
        {
            total += newRating.Value;
            listing.TotalReviews++;
        }

        // 🔹 HANDLE ZERO CASE
        if (listing.TotalReviews == 0)
        {
            listing.AverageRating = 0;
        }
        else
        {
            listing.AverageRating = total / listing.TotalReviews;
        }

        await _listingRepo.UpdateAsync(listing);
    }
}