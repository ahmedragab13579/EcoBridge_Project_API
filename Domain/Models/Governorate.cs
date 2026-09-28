using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domain.Models;

public class Governorate
{

    public int Id { get; set; }

    public int CountryId { get; set; }


    public string Code { get; set; } = string.Empty;


    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public Country Country { get; set; } = null!;

    public ICollection<City> Cities { get; set; } = new List<City>();
}
