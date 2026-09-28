using EcoBridge.Application.Features.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("GeneralTokenBucket")]
public class NotificationsController(INotificationService notificationService) : BaseApiController
{
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _notificationService.GetUserNotificationsAsync(
            CurrentUserId, page, pageSize, ct);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var result = await _notificationService.GetUnreadCountAsync(CurrentUserId, ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _notificationService.MarkAsReadAsync(id, CurrentUserId, ct);
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var result = await _notificationService.MarkAllAsReadAsync(CurrentUserId, ct);
        return ApiOk(result.Data!, result.Code);
    }
}