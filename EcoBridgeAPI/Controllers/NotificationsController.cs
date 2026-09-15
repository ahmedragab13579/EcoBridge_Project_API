using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
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
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, ct);
        return HandleResult(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _notificationService.GetUnreadCountAsync(userId, ct);
        return HandleResult(result);
    }

    [HttpPatch("{id}/read")]
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _notificationService.MarkAsReadAsync(id, userId, ct);
        return HandleResult(result);
    }

    [HttpPatch("read-all")]
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _notificationService.MarkAllAsReadAsync(userId, ct);
        return HandleResult(result);
    }
}
