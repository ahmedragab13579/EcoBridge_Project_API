using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Application.DTO.Users.Charity;

public class CharityProfileDTO : UserDTO
{

    public string? OrganizationName { get;  set; }

    public string? RegistrationNumber { get;  set; }
}
