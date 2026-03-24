using Microsoft.Extensions.Logging;
using TripService.Domain.Pricing;
using Shared.ValueObjects;

namespace TripService.Application.Services;

public class TripPricingCalculator
{
    private readonly IEnumerable<IPricingStrategy> _strategies;
    private readonly ILogger<TripPricingCalculator> _logger;

    public TripPricingCalculator(
        IEnumerable<IPricingStrategy> strategies,
        ILogger<TripPricingCalculator> logger)
    {
        _strategies = strategies;
        _logger     = logger;
    }

    public Money Calculate(PricingContext context)
    {
        decimal total = 0;

        foreach (var strategy in _strategies)
        {
            var cost = strategy.Calculate(context);
            total += cost.Amount;

            _logger.LogInformation(
                "Pricing strategy {Strategy}: +{Amount} {Currency}",
                strategy.Name, cost.Amount, cost.Currency);
        }

        _logger.LogInformation(
            "Total cost calculated: {Total} USD | Vehicle: {Type} | Distance: {Km}km | StartTime: {Start}",
            total, context.VehicleType, context.DistanceKm, context.StartTime);

        return new Money(Math.Round(total, 2), "USD");
    }
}