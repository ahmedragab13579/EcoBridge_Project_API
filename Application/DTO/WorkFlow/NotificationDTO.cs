using EcoBridge.Domain.Enums;

namespace EcoBridge.Application.DTO.WorkFlow;

public class NotificationDTO
{
    public int Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? RelatedEntityId { get; set; }
}
