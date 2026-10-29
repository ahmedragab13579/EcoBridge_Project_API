using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.DTO.WorkFlow;

public class UpdateDonationDTO
{
    public string FoodType { get; set; }

    public string Quantity { get; set; }

    public DateTime ExpiryDate { get; set; }

    public string PickupLocation { get; set; }

    public int CityId { get; set; }

    public IFormFile? Image { get; set; }
}
