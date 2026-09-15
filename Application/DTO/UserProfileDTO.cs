using EcoBridge.Domain.Enums;

namespace EcoBridge.Application.DTO;

public class UserProfileDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role  { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public int TotalWalletPoints { get; set; }
    public int TrustScore { get; set; } = 100;
    public int? CityId { get; set; }
    public string? VehicleType { get; set; }
    public string? Address { get; set; }
    public string? AddressDetails { get; set; }
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}
