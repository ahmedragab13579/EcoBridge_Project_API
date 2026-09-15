using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Domain.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
