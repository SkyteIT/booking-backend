using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Models;

namespace Ube.Infrastructure.Services;

// Automatically transitions Confirmed bookings whose EndDateTime has
// passed to Completed - previously this only ever happened via a
// manual Admin status override. A BackgroundService is a singleton, so
// it resolves the scoped IBookingService through a fresh scope on
// every run rather than injecting it directly.
public class BookingCompletionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BookingCompletionOptions _options;
    private readonly ILogger<BookingCompletionBackgroundService> _logger;

    public BookingCompletionBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<BookingCompletionOptions> options,
        ILogger<BookingCompletionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("BookingCompletionBackgroundService is disabled via configuration");
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
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var completed = await bookingService.CompleteExpiredBookingsAsync(ct);
            _logger.LogInformation("Booking completion sweep: completed {Count} booking(s)", completed);
        }
        catch (Exception ex)
        {
            // Never let a bad tick crash the hosted service - there's no
            // admin watching this in real time.
            _logger.LogError(ex, "Booking completion sweep failed");
        }
    }
}
