using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domain.Models;

public class City
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int GovernorateId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    [ForeignKey(nameof(GovernorateId))]
    public Governorate Governorate { get; set; } = null!;
}
