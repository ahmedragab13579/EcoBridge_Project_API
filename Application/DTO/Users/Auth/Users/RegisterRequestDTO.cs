using EcoBridge.Domain.Enums;

namespace EcoBridge.Application.DTO.Users.Auth.Users;

public class RegisterRequestDTO
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? CountryId { get; set; }
    public int? GovernorateId { get; set; }
    public int? CityId { get; set; }
    public string? AddressDetails { get; set; }
    public string? VehicleDetails { get; set; }
    public string? OrganizationName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? VehicleType { get; set; }
}
