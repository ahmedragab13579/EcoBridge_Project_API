namespace EcoBridge.Application.DTO;

public class RefreshTokenRequestDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
