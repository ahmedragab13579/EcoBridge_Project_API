using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Application.DTO.WorkFlow;

public class CreateDonationDTO
{
    public int CategoryId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string FoodType { get; set; }

    public string Quantity { get; set; } = string.Empty;

    public decimal? Weight { get; set; }

    public DateTime ExpiryDate { get; set; }


    public string PickupLocation { get; set; } = string.Empty;

    public int CityId { get; set; }

    public IFormFile? Image { get; set; }
}
