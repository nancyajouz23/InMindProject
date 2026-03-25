using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using Shared.Messages;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class DriverStatusChangedConsumer : IConsumer<DriverStatusChangedMessage>
{
    private readonly INotificationService _notifications;
    private readonly ILogger<DriverStatusChangedConsumer> _logger;

    public DriverStatusChangedConsumer(
        INotificationService notifications,
        ILogger<DriverStatusChangedConsumer> logger)
    {
        _notifications = notifications;
        _logger        = logger;
    }

    public async Task Consume(ConsumeContext<DriverStatusChangedMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "NOTIFICATION-SVC received DriverStatusChanged | DriverId: {DriverId} | Status: {Status}",
            msg.DriverId, msg.NewStatus);

        switch (msg.NewStatus)
        {
            case "Active":
                await _notifications.CreateAsync(
                    type: "DriverAvailable",
                    message: $"Driver {msg.DriverId} is now available.",
                    targetRole: "FleetManager",
                    driverId: msg.DriverId
                );
                break;

            case "OnTrip":
                await _notifications.CreateAsync(
                    type: "DriverOnTrip",
                    message: $"Driver {msg.DriverId} is now on trip.",
                    targetRole: "FleetManager",
                    driverId: msg.DriverId
                );
                break;

            case "Inactive":
                await _notifications.CreateAsync(
                    type: "DriverUnavailable",
                    message: $"Driver {msg.DriverId} is currently unavailable.",
                    targetRole: "FleetManager",
                    driverId: msg.DriverId
                );
                break;
            
            default:
                _logger.LogWarning(
                    "Unknown driver status: {Status} for DriverId: {DriverId}",
                    msg.NewStatus, msg.DriverId);
                break;
        }
    }
}