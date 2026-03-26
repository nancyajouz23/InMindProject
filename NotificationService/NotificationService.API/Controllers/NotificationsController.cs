using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using System.Security.Claims;
using Shared.Exceptions;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
        => _service = service;
    

    // Get my notifications based on my role.
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool? unreadOnly = null)
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (!roles.Any())
            return Unauthorized("No roles found in token.");

        var notifications = await _service.GetMyNotificationsAsync(roles, unreadOnly);
        return Ok(notifications);
    }
    
    // Get all notifications for a specific role.
    [HttpGet("role/{role}")]
    [Authorize(Roles = "Admin,FleetManager")]
    public async Task<IActionResult> GetByRole(
        [FromRoute] string role,
        [FromQuery] bool? unreadOnly = null)
    {
        var result = await _service.GetByRoleAsync(role, unreadOnly);
        return Ok(result);
    }
    
    [HttpGet("driver/{driverId:guid}")]
    [Authorize(Roles = "Admin,FleetManager")]
    public async Task<IActionResult> GetByDriverId(
        [FromRoute] Guid driverId,
        [FromQuery] bool? unreadOnly = null)
    {
        var result = await _service.GetByDriverIdAsync(driverId, unreadOnly);
        return Ok(result);
    }

    // Users can only mark notifications for roles they have except admin
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead([FromRoute] Guid id)
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        await _service.MarkAsReadAsync(id, roles);

        return NoContent();
    }
}