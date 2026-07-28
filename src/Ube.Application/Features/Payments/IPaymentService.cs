namespace Ube.Application.Features.Payments;

public interface IPaymentService
{
    Task<PaymentDto> InitiateAsync(Guid initiatedByUserId, InitiatePaymentRequest request, CancellationToken ct = default);
    Task<PaymentDto> GetAsync(Guid paymentId, Guid requestingUserId, bool isAdmin, CancellationToken ct = default);
}
