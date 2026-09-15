using EcoBridge.Domain.Enums;

namespace EcoBridge.Application.DTO;

public class RegisterRequestDTO
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? CountryId { get; set; }
    public int? GovernorateId { get; set; }
    public int? CityId { get; set; }
    public string? AddressDetails { get; set; }
}
