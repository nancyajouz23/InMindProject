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

        // Only notify when driver becomes inactive — requires FleetManager attention
        if (msg.NewStatus == "Inactive")
        {
            await _notifications.CreateAsync(
                type:       "DriverInactive",
                message:    $"Driver {msg.DriverId} has been deactivated and is no longer available.",
                targetRole: "FleetManager",
                driverId:   msg.DriverId
            );
        }
        
    }
}