using EcoBridge.Domain.Enums;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;

namespace EcoBridge.Application.Features.Notifications;

public interface INotificationService
{
    Task<Result<NotificationDTO>> CreateNotificationAsync(
        Guid recipientId,
        string title,
        string body,
        NotificationType type,
        Guid? relatedEntityId = null,
        CancellationToken ct = default);

    Task<Result<List<NotificationDTO>>> GetUserNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task<Result<bool>> MarkAsReadAsync(int notificationId, Guid userId, CancellationToken ct = default);

    Task<Result<bool>> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
