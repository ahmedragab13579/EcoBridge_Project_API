using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domain.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime ExpiresAtUtc { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? RevokedAtUtc { get; set; }

    [MaxLength(512)]
    public string? ReplacedByToken { get; set; }

    [NotMapped]
    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    public Account Account { get; set; } = null!;
}
