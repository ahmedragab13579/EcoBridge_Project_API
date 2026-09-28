using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Notifications;

public class NotificationService(
    IEcoBridgeDbContext dbContext,
    IHubContext<MissionHub> hubContext
) : INotificationService
{
    public async Task<ApiResult<NotificationDTO>> CreateNotificationAsync(
        Guid recipientId,
        string title,
        string body,
        NotificationType type,
        Guid? relatedEntityId = null,
        CancellationToken ct = default)
    {
        var notification = Notification.Create(recipientId, title, body, type, relatedEntityId);

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(ct);

        var dto = new NotificationDTO
        {
            Id = notification.Id,
            RecipientId = notification.RecipientId,
            Title = notification.Title,
            Body = notification.Body,
            Type = notification.Type.ToString(),
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc,
            RelatedEntityId = notification.RelatedEntityId
        };

        // Real-time direct push to user if online
        try
        {
            await hubContext.Clients
                .Group($"User_{recipientId}")
                .SendAsync("ReceiveNotification", dto, cancellationToken: ct);
        }
        catch
        {
            // Real-time delivery is best-effort; don't fail the request if the hub is unavailable.
        }

        return ApiResult<NotificationDTO>.Ok(dto, AppMessageCodes.NOTIFICATION_CREATED);
    }

    public async Task<ApiResult<PagedResult<NotificationDTO>>> GetUserNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var items = await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDTO
            {
                Id = n.Id,
                RecipientId = n.RecipientId,
                Title = n.Title,
                Body = n.Body,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc,
                RelatedEntityId = n.RelatedEntityId
            })
            .ToListAsync(ct);

        return ApiResult<PagedResult<NotificationDTO>>.Ok(
            new PagedResult<NotificationDTO>
            {
                 Items= items,
                 Page = page,
                 PageNumber = page,
                 PageSize = pageSize,
                 TotalCount = items.Count
            },
            AppMessageCodes.NOTIFICATIONS_RETRIEVED);
    }

    public async Task<ApiResult<int>> GetUnreadCountAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var count = await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .CountAsync(ct);

        return ApiResult<int>.Ok(count, AppMessageCodes.NOTIFICATIONS_COUNT_RETRIEVED);
    }

    public async Task<ApiResult<bool>> MarkAsReadAsync(
        int notificationId,
        Guid userId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var rowsAffected = await dbContext.Notifications
            .Where(n => n.Id == notificationId && n.RecipientId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

        if (rowsAffected == 0)
            throw new DomainException.NotFoundException(AppMessageCodes.NOTIFICATION_NOT_FOUND);

        return ApiResult<bool>.Ok(true, AppMessageCodes.NOTIFICATION_READ_SUCCESS);
    }

    public async Task<ApiResult<bool>> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        await dbContext.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

        return ApiResult<bool>.Ok(true, AppMessageCodes.NOTIFICATION_READ_SUCCESS);
    }
}