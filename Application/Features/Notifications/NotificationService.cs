using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Hubs;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Notifications;

public class NotificationService(
    IEcoBridgeDbContext dbContext,
    IHubContext<MissionHub> hubContext
) : INotificationService
{
    public async Task<Result<NotificationDTO>> CreateNotificationAsync(
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
            await hubContext.Clients.Group($"User_{recipientId}")
                .SendAsync("ReceiveNotification", dto, cancellationToken: ct);
        }
        catch
        {
            // Logging / ignored if client is disconnected
        }

        return Result<NotificationDTO>.SuccessResult(dto, AppMessageCodes.NOTIFICATION_CREATED);
    }

    public async Task<Result<List<NotificationDTO>>> GetUserNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
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

        return Result<List<NotificationDTO>>.SuccessResult(items, AppMessageCodes.NOTIFICATIONS_RETRIEVED);
    }

    public async Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var count = await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .CountAsync(ct);

        return Result<int>.SuccessResult(count, AppMessageCodes.NOTIFICATIONS_COUNT_RETRIEVED);
    }

    public async Task<Result<bool>> MarkAsReadAsync(int notificationId, Guid userId, CancellationToken ct = default)
    {
        var rowsAffected = await dbContext.Notifications
            .Where(n => n.Id == notificationId && n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

        if (rowsAffected == 0)
            return Result<bool>.FailResult(false, AppMessageCodes.NOTIFICATION_NOT_FOUND, ErrorType.NotFound);

        return Result<bool>.SuccessResult(true, AppMessageCodes.NOTIFICATION_READ_SUCCESS);
    }

    public async Task<Result<bool>> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        await dbContext.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

        return Result<bool>.SuccessResult(true, AppMessageCodes.NOTIFICATION_READ_SUCCESS);
    }
}
