namespace TripService.Domain.Pricing;

public class PricingContext
{
    public string   VehicleType  { get; init; } = null!;
    
    public decimal  DistanceKm   { get; init; }
    
    public DateTime StartTime    { get; init; }
    
    public DateTime EndTime      { get; init; }
    
    public double DurationMinutes => (EndTime - StartTime).TotalMinutes;
}