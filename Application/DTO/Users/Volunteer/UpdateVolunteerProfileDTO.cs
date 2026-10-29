using System.ComponentModel.DataAnnotations;
using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Application.DTO.Users.Volunteer;

public class UpdateVolunteerProfileDTO : MiniUpdateDto
{

    public string? VehicleType { get; set; }

    public string? VehicleDetails { get; set; }


}
