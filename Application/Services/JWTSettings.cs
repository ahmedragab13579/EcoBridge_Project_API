namespace EcoBridge.Application.Services;

public class JWTSettings
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int DurationInMinutes { get; set; }
    public int RefreshTokenDurationInDays { get; set; }
}
