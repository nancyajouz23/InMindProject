using Shared.ValueObjects;

namespace TripService.Domain.Pricing;

public interface IPricingStrategy
{
    // Name shown in logs so we know which strategy was used
    string Name { get; }

    Money Calculate(PricingContext context);
}