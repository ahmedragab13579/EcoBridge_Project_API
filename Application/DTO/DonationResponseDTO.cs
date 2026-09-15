namespace EcoBridge.Application.DTO;

public class DonationResponseDTO
{
    public Guid Id { get; set; }
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string FoodType { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid DonorId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
