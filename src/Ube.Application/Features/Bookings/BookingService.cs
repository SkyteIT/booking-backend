using Ube.Application.Common.Interfaces.Services;
using Ube.Domain.Enums.Bookings;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Common.Exceptions;

namespace Ube.Application.Features.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
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
            CanReject = BookingValidationRules.CanVendorReject(booking, vendorId).IsSuccess
        };
}
