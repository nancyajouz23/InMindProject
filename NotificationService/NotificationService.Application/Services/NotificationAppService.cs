using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Domain.Interfaces;
using Shared.Exceptions;

namespace NotificationService.Application.Services;

public class NotificationAppService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly ILogger<NotificationAppService> _logger;

    public NotificationAppService(INotificationRepository repo, ILogger<NotificationAppService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task CreateAsync(string type, string message, string targetRole,
                                  Guid? targetUserId = null, Guid? vehicleId = null,
                                  Guid? driverId = null, Guid? tripId = null)
    {
        var notification = Notification.Create(type, message, targetRole,
                                               targetUserId, vehicleId, driverId, tripId);
        await _repo.AddAsync(notification);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "NOTIFICATION CREATED | Type: {Type} | Role: {Role} | Message: {Message}",
            type, targetRole, message);
    }

    public async Task<NotificationDto?> GetByIdAsync(Guid id)
    {
        var notification = await _repo.GetByIdAsync(id);
        return notification == null ? null : ToDto(notification);
    }

    public async Task<IEnumerable<NotificationDto>> GetByRoleAsync(string role, bool? unreadOnly = null)
    {
        var items = await _repo.GetByRoleAsync(role, unreadOnly);
        return items.Select(ToDto);
    }

    public async Task<IEnumerable<NotificationDto>> GetByDriverIdAsync(Guid driverId, bool? unreadOnly = null)
    {
        var items = await _repo.GetByDriverIdAsync(driverId, unreadOnly);
        return items.Select(ToDto);
    }

    public async Task MarkAsReadAsync(Guid notificationId, IEnumerable<string> userRoles)
    {
        var notification = await _repo.GetByIdAsync(notificationId)
                           ?? throw new NotificationNotFoundException(notificationId);

        // Authorization logic moved here ✅
        bool isAuthorized = userRoles.Contains("Admin") ||
                            userRoles.Contains(notification.TargetRole);

        if (!isAuthorized)
        {
            throw new ForbiddenException(
                $"You cannot mark this notification as read. " +
                $"This notification is for role '{notification.TargetRole}' " +
                $"but you have roles: {string.Join(", ", userRoles)}");
        }

        notification.MarkAsRead();

        await _repo.UpdateAsync(notification);
        await _repo.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(
        IEnumerable<string> roles, bool? unreadOnly)
    {
        var notifications = new List<NotificationDto>();

        foreach (var role in roles.Where(r => ValidRoles.Contains(r)))
        {
            if (role == "Driver")
            {
                // Drivers only see their own notifications, not all drivers'
                var roleNotifications = await _repo.GetByRoleAsync(role, unreadOnly);
                notifications.AddRange(roleNotifications
                    .Where(n => n.TargetUserId == null)  // broadcast to all drivers
                    .Select(ToDto));
            }
            else
            {
                var roleNotifications = await _repo.GetByRoleAsync(role, unreadOnly);
                notifications.AddRange(roleNotifications.Select(ToDto));
            }
        }

        return notifications
            .DistinctBy(n => n.Id)
            .OrderByDescending(n => n.CreatedAt);
    }

    private static readonly string[] ValidRoles =
    {
        "Admin", "FleetManager", "Dispatcher", "Driver"
    };

    private static NotificationDto ToDto(Notification n) => new(
        n.Id, n.Type, n.Message, n.TargetRole,
        n.TargetUserId, n.IsRead, n.CreatedAt,
        n.VehicleId, n.DriverId, n.TripId);
}