namespace EcoBridge.Application.Contracts;

public interface ICookieService
{
    void SetTokenCookie(string accessToken, string refreshToken);
    void ClearTokenCookies();
    string? GetRefreshToken();
}
