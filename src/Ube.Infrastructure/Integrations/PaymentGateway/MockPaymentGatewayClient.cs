using Ube.Application.Features.Payments;

namespace Ube.Infrastructure.Integrations.PaymentGateway;

// Dev/testing only. Simulates a successful gateway synchronously so the
// rest of the Payments domain can be built and tested before the real
// gateway (supplied by a separate team) exists. Swapping it out later is a
// one-line DI change in Program.cs.
public class MockPaymentGatewayClient : IPaymentGatewayClient
{
    public Task<GatewayChargeResult> InitiateChargeAsync(decimal amount, string currency, string idempotencyKey, string reference, CancellationToken ct = default)
        => Task.FromResult(new GatewayChargeResult($"mock_charge_{idempotencyKey}", GatewayChargeStatus.Succeeded));

    public Task<GatewayChargeStatus> ConfirmChargeAsync(string externalReference, CancellationToken ct = default)
        => Task.FromResult(GatewayChargeStatus.Succeeded);

    public Task<GatewayRefundResult> InitiateRefundAsync(string externalChargeReference, decimal amount, string idempotencyKey, CancellationToken ct = default)
        => Task.FromResult(new GatewayRefundResult($"mock_refund_{idempotencyKey}", true));

    public bool VerifyWebhookSignature(string payload, string signatureHeader) => true;
}
