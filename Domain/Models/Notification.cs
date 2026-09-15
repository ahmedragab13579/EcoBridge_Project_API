using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models;

public class Notification
{
    [Key]
    public int Id { get; protected set; }

    [Required]
    public Guid RecipientId { get; protected set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; protected set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Body { get; protected set; } = string.Empty;

    [Required]
    [Column(TypeName = "tinyint")]
    public NotificationType Type { get; protected set; }

    [Required]
    public bool IsRead { get; protected set; } = false;

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public Guid? RelatedEntityId { get; protected set; }

    [ForeignKey(nameof(RecipientId))]
    public Account Recipient { get; set; } = null!;

    protected Notification() { }

    public static Notification Create(Guid recipientId, string title, string body, NotificationType type, Guid? relatedEntityId = null)
    {
        return new Notification
        {
            RecipientId = recipientId,
            Title = title,
            Body = body,
            Type = type,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
