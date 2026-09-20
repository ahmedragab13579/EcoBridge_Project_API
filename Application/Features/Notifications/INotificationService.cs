using EcoBridge.Domain.Enums;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Result;

namespace EcoBridge.Application.Features.Notifications;

public interface INotificationService
{
    Task<ApiResult<NotificationDTO>> CreateNotificationAsync(
        Guid recipientId,
        string title,
        string body,
        NotificationType type,
        Guid? relatedEntityId = null,
        CancellationToken ct = default);

    Task<ApiResult<List<NotificationDTO>>> GetUserNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<ApiResult<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task<ApiResult<bool>> MarkAsReadAsync(int notificationId, Guid userId, CancellationToken ct = default);

    Task<ApiResult<bool>> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
