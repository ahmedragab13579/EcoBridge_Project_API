using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Application.DTO;

public class UpdateProfileDTO
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public int? CityId { get; set; }

    [MaxLength(50)]
    public string? VehicleType { get; set; }

    [MaxLength(100)]
    public string? VehicleDetails { get; set; }

    [MaxLength(150)]
    public string? OrganizationName { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? AddressDetails { get; set; }

    public IFormFile? ProfileImage { get; set; }
}
