namespace NotificationService.Application.DTOs;
 
public record NotificationDto(
    Guid     Id,
    string   Type,
    string   Message,
    string   TargetRole,
    Guid?    TargetUserId,
    bool     IsRead,
    DateTime CreatedAt,
    Guid?    VehicleId,
    Guid?    DriverId,
    Guid?    TripId
);
 