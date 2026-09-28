using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Domain.Models;

public class Country
{
    public int Id { get; set; }


    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();


    private Country()
    {
        
    }
}
