using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domain.Models;

public class PenaltyLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public int Amount { get; set; }

    [Required]
    [MaxLength(100)]
    public string ReasonCode { get; set; } = string.Empty;

    public Guid? MissionId { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AccountId))]
    public Account Account { get; set; } = null!;

    [ForeignKey(nameof(MissionId))]
    public DeliveryMission? Mission { get; set; }
}
