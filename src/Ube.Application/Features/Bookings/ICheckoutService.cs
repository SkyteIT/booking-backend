namespace Ube.Application.Features.Bookings;

public interface ICheckoutService
{
    Task<CheckoutResultDto> CheckoutAsync(Guid customerId, CheckoutRequest request, CancellationToken ct = default);
}
