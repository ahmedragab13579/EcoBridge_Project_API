using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Application.DTO;

public class CreateDonationDTO
{
    public int CategoryId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    [MaxLength(100)]
    public string FoodType { get; set; }

    [Required]
    [MaxLength(50)]
    public string Quantity { get; set; } = string.Empty;

    public decimal? Weight { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [MaxLength(300)]
    public string PickupLocation { get; set; } = string.Empty;

    public int CityId { get; set; }

    public IFormFile? Image { get; set; }
}
