using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;

namespace EcoBridge.Domain.Models;

public class Notification
{

    public int Id { get; private set; }

    public Guid RecipientId { get; private set; }


    public string Title { get; private set; } = string.Empty;


    public string Body { get; private set; } = string.Empty;


    public NotificationType Type { get; private set; }

    public bool IsRead { get; private set; }


    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public DateTime? ReadAtUtc { get; private set; }

    public Guid? RelatedEntityId { get; private set; }

    // ─────────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────────
    [ForeignKey(nameof(RecipientId))]
    public Account Recipient { get; private set; } = null!;

    // ─────────────────────────────────────────────────────────────
    // EF Core constructor
    // ─────────────────────────────────────────────────────────────
    private Notification() { }

    // ─────────────────────────────────────────────────────────────
    // Factory Method — the only valid creation path
    // ─────────────────────────────────────────────────────────────
    public static Notification Create(
        Guid recipientId,
        string title,
        string body,
        NotificationType type,
        Guid? relatedEntityId = null)
    {
        if (recipientId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.NOTIFICATION_RECIPIENT_ID_REQUIRED);

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException.ValidationException(
                AppMessageCodes.NOTIFICATION_TITLE_REQUIRED);

        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException.ValidationException(
                AppMessageCodes.NOTIFICATION_BODY_REQUIRED);

        if (!Enum.IsDefined(type))
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_NOTIFICATION_TYPE);

        var trimmedTitle = title.Trim();
        var trimmedBody = body.Trim();

        if (trimmedTitle.Length >DomainConstants.Name.MaxNameLength )
            throw new DomainException.ValidationException(
                AppMessageCodes.NOTIFICATION_TITLE_TOO_LONG);

        if (trimmedBody.Length > DomainConstants.Body.MaxBodyLength)
            throw new DomainException.ValidationException(
                AppMessageCodes.NOTIFICATION_BODY_TOO_LONG);

        if (relatedEntityId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_RELATED_ENTITY_ID);

        return new Notification
        {
            RecipientId = recipientId,
            Title = trimmedTitle,
            Body = trimmedBody,
            Type = type,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Behavior: Mark as read (idempotent)
    // ─────────────────────────────────────────────────────────────
    public void MarkAsRead()
    {
        if (IsRead) return;

        IsRead = true;
        ReadAtUtc = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────────────────────────
    // Behavior: Mark as unread (idempotent)
    // ─────────────────────────────────────────────────────────────
    public void MarkAsUnread()
    {
        if (!IsRead) return;

        IsRead = false;
        ReadAtUtc = null;
    }

    // ─────────────────────────────────────────────────────────────
    // Domain queries (pure, no side effects)
    // ─────────────────────────────────────────────────────────────
    public bool IsUnread() => !IsRead;

    public bool BelongsTo(Guid recipientId) => RecipientId == recipientId;

    public bool References(Guid entityId) =>
        RelatedEntityId.HasValue && RelatedEntityId.Value == entityId;
}