using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Domain.Models;

public class Country
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    public ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();
}
