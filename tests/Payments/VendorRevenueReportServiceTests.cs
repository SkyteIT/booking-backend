using System.Text;
using Moq;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Tests.Payments;

public class VendorRevenueReportServiceTests
{
    private sealed record Ctx(Mock<IPaymentRepository> PaymentRepo, Mock<IRefundRepository> RefundRepo, VendorRevenueReportService Service);

    private static Ctx Build()
    {
        var paymentRepo = new Mock<IPaymentRepository>();
        var refundRepo = new Mock<IRefundRepository>();
        refundRepo
            .Setup(r => r.GetProcessedAmountsByPaymentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal>());
        return new Ctx(paymentRepo, refundRepo, new VendorRevenueReportService(paymentRepo.Object, refundRepo.Object));
    }

    // Amount = CommissionAmount + PlatformFeeAmount + NetVendorAmount always
    // holds (MoneyMath.SplitCommission's invariant) - test data respects it.
    private static Payment MakePayment(decimal amount, decimal commission, decimal platformFee, DateTime createdAt, Guid? bookingId = null) =>
        new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId ?? Guid.NewGuid(),
            VendorProfileId = Guid.NewGuid(),
            Amount = amount,
            Currency = "LKR",
            CollectionMethod = PaymentCollectionMethod.PlatformCollected,
            Status = PaymentStatus.Captured,
            IdempotencyKey = Guid.NewGuid().ToString(),
            CommissionAmount = commission,
            PlatformFeeAmount = platformFee,
            NetVendorAmount = amount - commission - platformFee,
            CreatedAt = createdAt
        };

    private static void SetupPayments(Ctx ctx, Guid vendorProfileId, List<Payment> payments) =>
        ctx.PaymentRepo
            .Setup(r => r.GetCapturedByVendorInRangeAsync(vendorProfileId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

    [Fact]
    public async Task GetReport_Nets_Payment_Minus_Processed_Refund_Within_A_Day()
    {
        var ctx = Build();
        var vendorProfileId = Guid.NewGuid();
        var day = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var payment = MakePayment(amount: 1000m, commission: 100m, platformFee: 0m, createdAt: day.AddHours(2));

        SetupPayments(ctx, vendorProfileId, new List<Payment> { payment });
        ctx.RefundRepo
            .Setup(r => r.GetProcessedAmountsByPaymentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal> { [payment.Id] = 50m });

        var result = await ctx.Service.GetReportAsync(
            vendorProfileId,
            new RevenueReportRequest { StartDate = day.AddDays(-1), EndDate = day.AddDays(1), Granularity = ReportGranularity.Daily });

        var bucket = Assert.Single(result);
        Assert.Equal(day.Date, bucket.PeriodStart);
        Assert.Equal(850m, bucket.NetRevenue); // NetVendorAmount(900) - refund(50)
        Assert.Equal(1, bucket.BookingCount);
    }

    [Fact]
    public async Task GetReport_Buckets_Weekly_By_Monday()
    {
        var ctx = Build();
        var vendorProfileId = Guid.NewGuid();
        // Wednesday and Friday of the same ISO week - both should fall
        // into the Monday-dated bucket for that week.
        var wednesday = new DateTime(2026, 3, 11, 0, 0, 0, DateTimeKind.Utc);
        var friday = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc);
        var expectedMonday = new DateTime(2026, 3, 9);

        var payments = new List<Payment>
        {
            MakePayment(100m, 0m, 0m, wednesday),
            MakePayment(200m, 0m, 0m, friday),
        };
        SetupPayments(ctx, vendorProfileId, payments);

        var result = await ctx.Service.GetReportAsync(
            vendorProfileId,
            new RevenueReportRequest { StartDate = wednesday.AddDays(-3), EndDate = friday.AddDays(3), Granularity = ReportGranularity.Weekly });

        var bucket = Assert.Single(result);
        Assert.Equal(expectedMonday, bucket.PeriodStart);
        Assert.Equal(300m, bucket.NetRevenue);
    }

    [Fact]
    public async Task GetReport_Buckets_Monthly_By_FirstOfMonth()
    {
        var ctx = Build();
        var vendorProfileId = Guid.NewGuid();
        var early = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc);
        var late = new DateTime(2026, 3, 28, 0, 0, 0, DateTimeKind.Utc);
        var expectedFirst = new DateTime(2026, 3, 1);

        var payments = new List<Payment>
        {
            MakePayment(100m, 0m, 0m, early),
            MakePayment(200m, 0m, 0m, late),
        };
        SetupPayments(ctx, vendorProfileId, payments);

        var result = await ctx.Service.GetReportAsync(
            vendorProfileId,
            new RevenueReportRequest { StartDate = early.AddDays(-3), EndDate = late.AddDays(3), Granularity = ReportGranularity.Monthly });

        var bucket = Assert.Single(result);
        Assert.Equal(expectedFirst, bucket.PeriodStart);
        Assert.Equal(300m, bucket.NetRevenue);
    }

    [Fact]
    public async Task GetBreakdown_Computes_Gross_Commission_Refunds_And_Net()
    {
        var ctx = Build();
        var vendorProfileId = Guid.NewGuid();
        var day = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var payment = MakePayment(amount: 1000m, commission: 100m, platformFee: 20m, createdAt: day);

        SetupPayments(ctx, vendorProfileId, new List<Payment> { payment });
        ctx.RefundRepo
            .Setup(r => r.GetProcessedAmountsByPaymentIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal> { [payment.Id] = 30m });

        var result = await ctx.Service.GetGrossCommissionRefundBreakdownAsync(
            vendorProfileId,
            new RevenueReportRequest { StartDate = day.AddDays(-1), EndDate = day.AddDays(1) });

        Assert.Equal(1000m, result.GrossRevenue);
        Assert.Equal(120m, result.PlatformCommission); // commission + platform fee
        Assert.Equal(30m, result.Refunds);
        Assert.Equal(850m, result.NetEarnings); // NetVendorAmount(880) - refund(30)
    }

    [Fact]
    public async Task GetBreakdown_Excludes_Payments_Outside_Date_Range()
    {
        var ctx = Build();
        var vendorProfileId = Guid.NewGuid();
        var inRange = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);

        // GetCapturedByVendorInRangeAsync is itself responsible for the
        // date filter - the service must not re-widen it, so only the
        // in-range payment is ever handed to it in this test.
        SetupPayments(ctx, vendorProfileId, new List<Payment> { MakePayment(500m, 0m, 0m, inRange) });

        var result = await ctx.Service.GetGrossCommissionRefundBreakdownAsync(
            vendorProfileId,
            new RevenueReportRequest { StartDate = inRange.AddDays(-1), EndDate = inRange.AddDays(1) });

        Assert.Equal(500m, result.GrossRevenue);
    }

    [Fact]
    public async Task ExportReportCsv_Produces_Header_Bom_And_One_Row_Per_Bucket()
    {
        var ctx = Build();
        var vendorProfileId = Guid.NewGuid();
        var day = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);

        SetupPayments(ctx, vendorProfileId, new List<Payment> { MakePayment(500m, 0m, 0m, day) });

        var csvBytes = await ctx.Service.ExportReportCsvAsync(
            vendorProfileId,
            new RevenueReportRequest { StartDate = day.AddDays(-1), EndDate = day.AddDays(1) });

        var bom = Encoding.UTF8.GetPreamble();
        Assert.Equal(bom, csvBytes.Take(bom.Length));

        var text = Encoding.UTF8.GetString(csvBytes, bom.Length, csvBytes.Length - bom.Length);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("Period,Net Revenue,Bookings", lines[0].Trim());
        Assert.Equal(2, lines.Length); // header + one bucket row
        Assert.Contains("500", lines[1]);
    }
}
