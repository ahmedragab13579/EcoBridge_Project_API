using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Application.DTO.Users.Volunteer;

public class VolunteerProfileDTO : UserDTO
{

    public string? VehicleType { get; set; }
    public string? VehicleDetails { get; set; }
}
