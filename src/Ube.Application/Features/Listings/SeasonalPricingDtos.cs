using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class SeasonalPricingRuleDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid? ListingUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public SeasonalRateAdjustmentType AdjustmentType { get; set; }
    public decimal AdjustmentValue { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSeasonalPricingRuleRequest
{
    public Guid? ListingUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public SeasonalRateAdjustmentType AdjustmentType { get; set; }
    public decimal AdjustmentValue { get; set; }
}

public class UpdateSeasonalPricingRuleRequest
{
    public string? Name { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public SeasonalRateAdjustmentType? AdjustmentType { get; set; }
    public decimal? AdjustmentValue { get; set; }
    public bool? IsActive { get; set; }
}

public class PriceQuoteDto
{
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
