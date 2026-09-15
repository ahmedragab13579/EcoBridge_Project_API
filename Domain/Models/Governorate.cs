using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domain.Models;

public class Governorate
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CountryId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    [ForeignKey(nameof(CountryId))]
    public Country Country { get; set; } = null!;

    public ICollection<City> Cities { get; set; } = new List<City>();
}
