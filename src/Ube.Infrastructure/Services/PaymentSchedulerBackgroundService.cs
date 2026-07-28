using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ube.Application.Common.Models;
using Ube.Application.Features.Payments;

namespace Ube.Infrastructure.Services;

// Runs the two Payments-domain sweeps that previously had to be triggered
// manually by an admin: flagging overdue vendor commission invoices
// (suspending the vendor), and reconciling recent payments against the
// gateway. A BackgroundService is a singleton, so it resolves the scoped
// services it needs through a fresh scope on every run rather than
// injecting them directly.
public class PaymentSchedulerBackgroundService : BackgroundService
{
    // Audit entries from this job are attributed to this well-known
    // sentinel rather than a real user - there is no human "actor" for a
    // scheduled run.
    public static readonly Guid SystemActorId = Guid.Empty;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PaymentSchedulerOptions _options;
    private readonly ILogger<PaymentSchedulerBackgroundService> _logger;

    public PaymentSchedulerBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<PaymentSchedulerOptions> options,
        ILogger<PaymentSchedulerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("PaymentSchedulerBackgroundService is disabled via configuration");
            return;
        }

        var interval = TimeSpan.FromHours(Math.Max(1, _options.RunIntervalHours));
        using var timer = new PeriodicTimer(interval);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        try
        {
            var invoiceService = scope.ServiceProvider.GetRequiredService<IVendorInvoiceService>();
            var overdue = await invoiceService.ProcessOverdueAsync(SystemActorId, ct);
            _logger.LogInformation("Payment scheduler: flagged {Count} overdue vendor invoice(s)", overdue.Count);
        }
        catch (Exception ex)
        {
            // One sweep failing must never stop the other, or crash the
            // whole background service - there's no admin watching this
            // in real time to catch a startup failure.
            _logger.LogError(ex, "Payment scheduler: overdue-invoice sweep failed");
        }

        try
        {
            var reconciliationService = scope.ServiceProvider.GetRequiredService<IPaymentReconciliationService>();
            var periodEnd = DateTime.UtcNow;
            var periodStart = periodEnd.AddHours(-Math.Max(1, _options.ReconciliationLookbackHours));
            var report = await reconciliationService.ReconcileAsync(SystemActorId, periodStart, periodEnd, ct);
            _logger.LogInformation(
                "Payment scheduler: reconciled {Checked} payment(s), {Mismatches} mismatch(es)",
                report.PaymentsChecked, report.Mismatches.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment scheduler: reconciliation sweep failed");
        }
    }
}
