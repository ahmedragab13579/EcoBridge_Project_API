namespace EcoBridge.Application.DTO.Users.Auth.Users;

public class RefreshTokenRequestDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
