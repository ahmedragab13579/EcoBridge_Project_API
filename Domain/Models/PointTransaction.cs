using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models;

public class PointTransaction
{
    [Key]
    public int Id { get; protected set; }

    [Required]
    public Guid UserId { get; protected set; }

    [Required]
    public int Amount { get; protected set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public TransactionType TransactionType { get; protected set; }

    public Guid? ReferenceId { get; protected set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public Account User { get; set; } = null!;

    protected PointTransaction() { }

    public static PointTransaction Create(Guid userId, int amount, TransactionType transactionType, Guid? referenceId = null)
    {
        return new PointTransaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = transactionType,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
