using Ube.Application.Common.Interfaces.Services;
using Ube.Domain.Enums.Bookings;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Reviews;

namespace Ube.Application.Features.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReviewRepository _reviewRepository;

    public BookingService(IBookingRepository bookingRepository, IUnitOfWork unitOfWork, IReviewRepository reviewRepository)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _reviewRepository = reviewRepository;
    }

    public async Task<BookingDetailDto> UpdateVendorBookingStatusAsync(Guid bookingId, Guid vendorId, BookingStatus newStatus)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking not found");

        var validationResult = newStatus switch
        {
            BookingStatus.Confirmed => BookingValidationRules.CanVendorConfirm(booking, vendorId),
            BookingStatus.Rejected  => BookingValidationRules.CanVendorReject(booking, vendorId),
            _ => Result.Failure("Invalid status transition for vendor")
        };

        if (!validationResult.IsSuccess)
            throw new BusinessRuleException(validationResult.ErrorMessage);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return MapToDetail(booking, vendorId);
    }

    public async Task<PagedResult<VendorBookingDto>> GetVendorBookingsAsync(Guid vendorId, BookingsRequest request)
    {
        var bookings = await _bookingRepository.GetBookingsByVendorIdAsync(vendorId, request);
        var vendorBookings = bookings.Items.Select(b => new VendorBookingDto
        {
            BookingId = b.Id,
            BookingNumber = b.BookingNumber,
            ListingTitle = b.Listing.Title,
            CustomerName = b.Customer.FirstName + " " + b.Customer.LastName,
            StartDateTime = b.StartDateTime,
            EndDateTime = b.EndDateTime,
            Status = b.Status,
            TotalAmount = b.TotalAmount,
            Currency = b.Currency,
            CreatedAt = b.CreatedAt
        }).ToList();

        return new PagedResult<VendorBookingDto>
        {
            Items = vendorBookings,
            PageNumber = bookings.PageNumber,
            PageSize = bookings.PageSize,
            TotalCount = bookings.TotalCount,
            TotalPages = bookings.TotalPages
        };
    }

    public async Task<BookingDetailDto> GetBookingDetailAsync(Guid bookingId, Guid vendorId)
    {
        var booking = await _bookingRepository.GetBookingAsync(bookingId, vendorId)
            ?? throw new NotFoundException("Booking not found or you do not have access to it");

        return MapToDetail(booking, vendorId);
    }

    public async Task<PagedResult<VendorBookingDto>> GetCustomerBookingsAsync(Guid customerId, BookingsRequest request)
    {
        var bookings = await _bookingRepository.GetBookingsByCustomerIdAsync(customerId, request);
        var customerBookings = bookings.Items.Select(b => new VendorBookingDto
        {
            BookingId = b.Id,
            BookingNumber = b.BookingNumber,
            ListingTitle = b.Listing.Title,
            CustomerName = b.Customer.FirstName + " " + b.Customer.LastName,
            StartDateTime = b.StartDateTime,
            EndDateTime = b.EndDateTime,
            Status = b.Status,
            TotalAmount = b.TotalAmount,
            Currency = b.Currency,
            CreatedAt = b.CreatedAt
        }).ToList();

        return new PagedResult<VendorBookingDto>
        {
            Items = customerBookings,
            PageNumber = bookings.PageNumber,
            PageSize = bookings.PageSize,
            TotalCount = bookings.TotalCount,
            TotalPages = bookings.TotalPages
        };
    }

    public async Task<BookingDetailDto> GetCustomerBookingDetailAsync(Guid bookingId, Guid customerId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking not found");

        if (booking.CustomerId != customerId)
            throw new NotFoundException("Booking not found");

        var canReview = booking.Status == BookingStatus.Completed
            && !await _reviewRepository.ExistsByBookingIdAsync(bookingId);

        return MapToCustomerDetail(booking, customerId, canReview);
    }

    public async Task<BookingDetailDto> CancelBookingAsync(Guid bookingId, Guid customerId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking not found");

        var validationResult = booking.Status == BookingStatus.Pending
            ? BookingValidationRules.CanUserCancelPendding(booking, customerId)
            : BookingValidationRules.CanUserCancelConfirmed(booking, customerId);

        if (!validationResult.IsSuccess)
            throw new BusinessRuleException(validationResult.ErrorMessage);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        // Just transitioned to Cancelled, was never Completed - never reviewable.
        return MapToCustomerDetail(booking, customerId, canReview: false);
    }

    public async Task<int> CompleteExpiredBookingsAsync(CancellationToken ct = default)
    {
        var expired = await _bookingRepository.GetBookingsPastEndDateAsync(DateTime.UtcNow, ct);
        var completedCount = 0;

        foreach (var booking in expired)
        {
            // The repo query already filters to Confirmed + past end date,
            // but re-validate through the same state machine used
            // everywhere else - never a raw status assignment.
            if (!BookingValidationRules.CanSystemComplete(booking).IsSuccess)
                continue;

            // Each booking gets its own transaction so one bad row can't
            // abort the rest of the sweep - same isolation the scheduled
            // Payments sweeps rely on (each sweep step is independently
            // try/caught), just per-item instead of per-sweep here.
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                booking.Status = BookingStatus.Completed;
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(booking);
                await _unitOfWork.CommitAsync();
                completedCount++;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
            }
        }

        return completedCount;
    }

    // CanUserCancelConfirmed has no StartDateTime check - a customer can
    // still cancel a Confirmed booking minutes before it starts. Existing
    // behavior of the already-tested rule, not introduced here.
    private static BookingDetailDto MapToCustomerDetail(Domain.Entities.Bookings.Booking booking, Guid customerId, bool canReview) =>
        new BookingDetailDto
        {
            BookingId = booking.Id,
            BookingNumber = booking.BookingNumber,
            ListingTitle = booking.Listing.Title,
            CustomerName = booking.Customer.FirstName + " " + booking.Customer.LastName,
            CustomerEmail = booking.Customer.Email,
            StartDateTime = booking.StartDateTime,
            EndDateTime = booking.EndDateTime,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            Currency = booking.Currency,
            CreatedAt = booking.CreatedAt,
            CanConfirm = false,
            CanReject = false,
            CanCancel = booking.Status == BookingStatus.Pending
                ? BookingValidationRules.CanUserCancelPendding(booking, customerId).IsSuccess
                : BookingValidationRules.CanUserCancelConfirmed(booking, customerId).IsSuccess,
            CanReview = canReview
        };

    private static BookingDetailDto MapToDetail(Domain.Entities.Bookings.Booking booking, Guid vendorId) =>
        new BookingDetailDto
        {
            BookingId = booking.Id,
            BookingNumber = booking.BookingNumber,
            ListingTitle = booking.Listing.Title,
            CustomerName = booking.Customer.FirstName + " " + booking.Customer.LastName,
            CustomerEmail = booking.Customer.Email,
            StartDateTime = booking.StartDateTime,
            EndDateTime = booking.EndDateTime,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            Currency = booking.Currency,
            CreatedAt = booking.CreatedAt,
            CanConfirm = BookingValidationRules.CanVendorConfirm(booking, vendorId).IsSuccess,
            CanReject = BookingValidationRules.CanVendorReject(booking, vendorId).IsSuccess,
            // A vendor can't review their own listing (ReviewRules.PreventReviewOwnBusiness
            // already blocks this server-side too) - never show the button on their own view.
            CanReview = false
        };
}
