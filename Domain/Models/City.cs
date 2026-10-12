using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domain.Models;

public class City
{
    public int Id { get; set; }

    public int GovernorateId { get; set; }


    public string Code { get; set; } = string.Empty;


    public string NameAr { get; set; } = string.Empty;


    public string NameEn { get; set; } = string.Empty;

    public Governorate Governorate { get; set; } = null!;

    public City()
    {
        
    }
}
