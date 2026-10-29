using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.DTO.Users.Auth.Users;

public class ChangePasswordDTO
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
