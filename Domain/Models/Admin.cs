using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domain.Models;

public class Admin
{
    [Key]
    [ForeignKey(nameof(Account))]
    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;
}
