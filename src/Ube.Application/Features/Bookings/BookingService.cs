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
    public async Task<BookingDetailDto> CreateBookingAsync(
    Guid customerId,
    CreateBookingRequest request)
    {
        var listing = await _bookingRepository.GetByListingIdAsync(request.ListingId);

        if (listing == null)
            throw new NotFoundException("Listing not found");

        if (!listing.IsActive)
            throw new BusinessRuleException("This listing is not available");

        if (request.StartDateTime >= request.EndDateTime)
            throw new BusinessRuleException("End date/time must be after start date/time");

        var existingBookings =
            await _bookingRepository.GetBookingsByListingAndDateRangeAsync(
                request.ListingId,
                request.StartDateTime,
                request.EndDateTime);

        if (existingBookings.Any())
            throw new BusinessRuleException(
                "This listing is already booked for the selected period");

        var sequence = await _bookingRepository.GetNextBookingSequenceAsync();

        var booking = new Domain.Entities.Bookings.Booking
        {
            Id = Guid.NewGuid(),
            BookingNumber = $"BKG-{sequence:D6}",
            ListingId = listing.Id,
            CustomerId = customerId,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            Status = BookingStatus.Pending,
            TotalAmount = listing.Price,
            Currency = listing.Currency,
            CreatedAt = DateTime.UtcNow
        };

        await _bookingRepository.AddAsync(booking);

        return new BookingDetailDto
        {
            BookingId = booking.Id,
            BookingNumber = booking.BookingNumber,
            ListingTitle = listing.Title,
            CustomerName = string.Empty,
            CustomerEmail = string.Empty,
            StartDateTime = booking.StartDateTime,
            EndDateTime = booking.EndDateTime,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            Currency = booking.Currency,
            CreatedAt = booking.CreatedAt,
            CanConfirm = false,
            CanReject = false
        };
    }

    public async Task<PagedResult<CustomerBookingDto>> GetCustomerBookingsAsync(
        Guid customerId,
        BookingsRequest request)
    {
        var bookings =
            await _bookingRepository.GetBookingsByCustomerIdAsync(
                customerId,
                request);

        var items = bookings.Items.Select(b => new CustomerBookingDto
        {
            BookingId = b.Id,
            BookingNumber = b.BookingNumber,
            ListingTitle = b.Listing.Title,
            StartDateTime = b.StartDateTime,
            EndDateTime = b.EndDateTime,
            Status = b.Status,
            TotalAmount = b.TotalAmount,
            Currency = b.Currency,
            CreatedAt = b.CreatedAt
        }).ToList();

        return new PagedResult<CustomerBookingDto>
        {
            Items = items,
            PageNumber = bookings.PageNumber,
            PageSize = bookings.PageSize,
            TotalCount = bookings.TotalCount,
            TotalPages = bookings.TotalPages
        };
    }

    public async Task<BookingDetailDto> GetCustomerBookingDetailAsync(
        Guid bookingId,
        Guid customerId)
    {
        var booking =
            await _bookingRepository.GetCustomerBookingAsync(
                bookingId,
                customerId)
            ?? throw new NotFoundException(
                "Booking not found or you do not have access to it");

        return MapToCustomerDetail(booking);
    }

    public async Task<BookingDetailDto> CancelCustomerBookingAsync(
        Guid bookingId,
        Guid customerId)
    {
        var booking =
            await _bookingRepository.GetCustomerBookingAsync(
                bookingId,
                customerId)
            ?? throw new NotFoundException(
                "Booking not found or you do not have access to it");

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new BusinessRuleException(
                "This booking cannot be cancelled");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);

        return MapToCustomerDetail(booking);
    }

    private static BookingDetailDto MapToCustomerDetail(
        Domain.Entities.Bookings.Booking booking)
    {
        return new BookingDetailDto
        {
            BookingId = booking.Id,
            BookingNumber = booking.BookingNumber,
            ListingTitle = booking.Listing.Title,
            CustomerName =
                $"{booking.Customer.FirstName} {booking.Customer.LastName}",
            CustomerEmail = booking.Customer.Email,
            StartDateTime = booking.StartDateTime,
            EndDateTime = booking.EndDateTime,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            Currency = booking.Currency,
            CreatedAt = booking.CreatedAt,
            CanConfirm = false,
            CanReject = false
        };
    }
    
}
