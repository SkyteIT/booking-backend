using Ube.Domain.Enums.Listings;
namespace Ube.Application.Features.Availability.Strategies;

public class StrategySelector
{
    private readonly Dictionary<AvailabilityType, IAvailabilityStrategy> _st;
    public StrategySelector(IEnumerable<IAvailabilityStrategy> st)
    {
        // Convert list to dictionary 
        _st = st.ToDictionary(s => s.Type);
    }
    // Get strategy by type (use this in service to get the right strategy to calculate availability)
    public IAvailabilityStrategy GetStrategy(AvailabilityType type)
    {
        if (_st.TryGetValue(type, out var strategy))
        {
            return strategy;
        }

        throw new NotSupportedException($"Strategy not found for type {type}");
    }
}