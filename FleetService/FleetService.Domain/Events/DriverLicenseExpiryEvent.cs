namespace FleetService.Domain.Events;

public record DriverLicenseExpiryEvent
(
    Guid     DriverId,
    DateTime LicenseExpiry
);