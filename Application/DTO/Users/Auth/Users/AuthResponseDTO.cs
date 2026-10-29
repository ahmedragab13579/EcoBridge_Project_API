namespace EcoBridge.Application.DTO.Users.Auth.Users;

public class AuthResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
}
