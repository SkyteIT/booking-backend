using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Fraud;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Fraud;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Payments;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Fraud;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly IFraudFlagRepository _flagRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPaymentService _paymentService;
    private readonly IAdminAlertService _adminAlertService;
    private readonly FraudDetectionOptions _options;
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(
        IFraudFlagRepository flagRepo,
        IBookingRepository bookingRepo,
        IUserRepository userRepo,
        IPaymentService paymentService,
        IAdminAlertService adminAlertService,
        IOptions<FraudDetectionOptions> options,
        ILogger<FraudDetectionService> logger)
    {
        _flagRepo = flagRepo;
        _bookingRepo = bookingRepo;
        _userRepo = userRepo;
        _paymentService = paymentService;
        _adminAlertService = adminAlertService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> IsNewAccountHighValueAsync(Guid customerId, decimal amount, CancellationToken ct = default)
    {
        if (!_options.Enabled) return false;
        if (amount < _options.HighValueThreshold) return false;

        var customer = await _userRepo.GetByIdAsync(customerId)
            ?? throw new NotFoundException("Customer not found");

        return customer.CreatedAt >= DateTime.UtcNow.AddHours(-_options.NewAccountHours);
    }

    public async Task CreateHoldFlagAsync(Booking booking, BookingStatus resolvedStatus, PaymentCollectionMethod collectionMethod, CancellationToken ct = default)
    {
        var flag = new FraudFlag
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            RuleTriggered = FraudRuleType.NewAccountHighValue,
            Severity = FraudFlagSeverity.Hold,
            Details = $"New account (created {booking.CreatedAt:yyyy-MM-dd}) booking {booking.TotalAmount:N2} {booking.Currency} exceeds the high-value threshold.",
            ResolvedStatusOnClear = resolvedStatus,
            CollectionMethodOnClear = collectionMethod
        };

        await _flagRepo.AddAsync(flag, ct);
        await NotifyAdminsAsync(flag.RuleTriggered, flag.Details, ct);
    }

    public async Task EvaluateFlagOnlyRulesAsync(Guid customerId, Guid bookingId, CancellationToken ct = default)
    {
        if (!_options.Enabled) return;

        try
        {
            var velocitySince = DateTime.UtcNow.AddMinutes(-_options.VelocityWindowMinutes);
            var recentCount = await _bookingRepo.CountByCustomerSinceAsync(customerId, velocitySince, ct);
            if (recentCount >= _options.MaxBookingsPerWindow)
            {
                var details = $"{recentCount} bookings created in the last {_options.VelocityWindowMinutes} minutes.";
                await _flagRepo.AddAsync(new FraudFlag
                {
                    Id = Guid.NewGuid(),
                    BookingId = bookingId,
                    CustomerId = customerId,
                    RuleTriggered = FraudRuleType.BookingVelocity,
                    Severity = FraudFlagSeverity.FlagOnly,
                    Details = details
                }, ct);
                await NotifyAdminsAsync(FraudRuleType.BookingVelocity, details, ct);
            }

            var cancellationSince = DateTime.UtcNow.AddDays(-_options.CancellationWindowDays);
            var cancelledCount = await _bookingRepo.CountCancelledByCustomerSinceAsync(customerId, cancellationSince, ct);
            if (cancelledCount > _options.MaxCancellationsPerWindow)
            {
                var details = $"{cancelledCount} bookings cancelled in the last {_options.CancellationWindowDays} days.";
                await _flagRepo.AddAsync(new FraudFlag
                {
                    Id = Guid.NewGuid(),
                    BookingId = bookingId,
                    CustomerId = customerId,
                    RuleTriggered = FraudRuleType.RepeatedCancellations,
                    Severity = FraudFlagSeverity.FlagOnly,
                    Details = details
                }, ct);
                await NotifyAdminsAsync(FraudRuleType.RepeatedCancellations, details, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fraud flag-only rule evaluation failed for booking {BookingId}", bookingId);
        }
    }

    private Task NotifyAdminsAsync(FraudRuleType rule, string details, CancellationToken ct) =>
        _adminAlertService.NotifyRolesAsync(
            new[] { UserRole.Admin, UserRole.SuperAdmin },
            "Fraud flag raised",
            $"{rule}: {details}",
            NotificationType.FraudFlagRaised,
            ct);

    public async Task<PagedResult<AdminFraudFlagDto>> GetPagedAsync(FraudFlagStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _flagRepo.GetPagedAsync(status, pageNumber, pageSize, ct);

        var mapped = items.Select(x => new AdminFraudFlagDto
        {
            Id = x.Flag.Id,
            BookingId = x.Flag.BookingId,
            BookingNumber = x.BookingNumber,
            ListingTitle = x.ListingTitle,
            CustomerName = x.CustomerName,
            RuleTriggered = x.Flag.RuleTriggered,
            Severity = x.Flag.Severity,
            Details = x.Flag.Details,
            Status = x.Flag.Status,
            CreatedAt = x.Flag.CreatedAt,
            ReviewedByUserId = x.Flag.ReviewedByUserId,
            ReviewedAt = x.Flag.ReviewedAt,
            ReviewNotes = x.Flag.ReviewNotes
        }).ToList();

        return new PagedResult<AdminFraudFlagDto>
        {
            Items = mapped,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<FraudFlagDto> ReviewAsync(Guid actorUserId, Guid flagId, ReviewFraudFlagRequest request, CancellationToken ct = default)
    {
        var flag = await _flagRepo.GetByIdAsync(flagId, ct)
            ?? throw new NotFoundException("Fraud flag not found");

        if (flag.Status != FraudFlagStatus.Open)
            throw new BusinessRuleException("Only an open fraud flag can be reviewed");

        if (flag.Severity == FraudFlagSeverity.Hold)
        {
            var booking = await _bookingRepo.GetByIdAsync(flag.BookingId)
                ?? throw new NotFoundException("Booking not found");

            if (request.Decision == FraudReviewDecision.Clear)
            {
                booking.Status = flag.ResolvedStatusOnClear ?? BookingStatus.Pending;
                booking.IsHeldForFraudReview = false;
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepo.UpdateAsync(booking);

                var paymentRequest = new InitiatePaymentRequest
                {
                    BookingId = booking.Id,
                    CollectionMethod = flag.CollectionMethodOnClear ?? PaymentCollectionMethod.PlatformCollected,
                    IdempotencyKey = $"fraud-clear:{flag.Id}"
                };
                await _paymentService.InitiateAsync(booking.CustomerId, paymentRequest, ct);
            }
            else
            {
                if (!BookingTransitionRules.CanAdminTransition(booking.Status, BookingStatus.Rejected))
                    throw new BusinessRuleException($"Cannot reject booking from status {booking.Status}");

                booking.Status = BookingStatus.Rejected;
                booking.IsHeldForFraudReview = false;
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepo.UpdateAsync(booking);
            }
        }

        flag.Status = request.Decision == FraudReviewDecision.Clear
            ? FraudFlagStatus.Cleared
            : FraudFlagStatus.ConfirmedFraud;
        flag.ReviewedByUserId = actorUserId;
        flag.ReviewedAt = DateTime.UtcNow;
        flag.ReviewNotes = request.Notes;
        await _flagRepo.UpdateAsync(flag, ct);

        return new FraudFlagDto
        {
            Id = flag.Id,
            BookingId = flag.BookingId,
            RuleTriggered = flag.RuleTriggered,
            Severity = flag.Severity,
            Details = flag.Details,
            Status = flag.Status,
            CreatedAt = flag.CreatedAt,
            ReviewedByUserId = flag.ReviewedByUserId,
            ReviewedAt = flag.ReviewedAt,
            ReviewNotes = flag.ReviewNotes
        };
    }
}
