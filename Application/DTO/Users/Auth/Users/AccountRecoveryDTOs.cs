using System.ComponentModel.DataAnnotations;

namespace EcoBridge.Application.DTO.Users.Auth.Users;

public class ForgotPasswordRequestDTO
{

    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDTO
{

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}

public class VerifyEmailRequestDTO
{

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}
