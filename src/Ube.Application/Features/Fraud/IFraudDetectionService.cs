using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Fraud;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Fraud;

public interface IFraudDetectionService
{
    // Pre-commit: does this new booking's account age + amount combination
    // require holding for admin review? Called by CheckoutService before it
    // decides the booking's initial Status.
    Task<bool> IsNewAccountHighValueAsync(Guid customerId, decimal amount, CancellationToken ct = default);

    // Writes the Hold-severity flag once CheckoutService has already forced
    // the booking to Pending/IsHeldForFraudReview and skipped payment
    // initiation. resolvedStatus/collectionMethod are what checkout would
    // have used had the booking not been held - stored so ReviewAsync(Clear)
    // can resolve it later without re-deriving Category rules.
    Task CreateHoldFlagAsync(Booking booking, BookingStatus resolvedStatus, PaymentCollectionMethod collectionMethod, CancellationToken ct = default);

    // Post-commit, weaker pattern signals (velocity, repeated cancellations).
    // Never throws - internally logs and swallows, since a bug here must
    // never break a checkout that already succeeded.
    Task EvaluateFlagOnlyRulesAsync(Guid customerId, Guid bookingId, CancellationToken ct = default);

    Task<PagedResult<AdminFraudFlagDto>> GetPagedAsync(FraudFlagStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<FraudFlagDto> ReviewAsync(Guid actorUserId, Guid flagId, ReviewFraudFlagRequest request, CancellationToken ct = default);
}
