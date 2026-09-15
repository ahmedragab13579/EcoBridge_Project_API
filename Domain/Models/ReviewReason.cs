using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models;

public class ReviewReason
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "tinyint")]
    public UserRole FromRole { get; set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public UserRole ToRole { get; set; }

    [Required]
    public bool IsPositive { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
