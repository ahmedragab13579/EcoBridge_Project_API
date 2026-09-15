using EcoBridge.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Infrastructure.Services.Implementation;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetTokenCookie(string accessToken, string refreshToken)
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null)
        {
            var cookieOptions = GetBaseCookieOptions();
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);

            response.Cookies.Append("AccessToken", accessToken, cookieOptions);
            response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }
    }

    public void ClearTokenCookies()
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null)
        {
            var cookieOptions = GetBaseCookieOptions();

            response.Cookies.Delete("AccessToken", cookieOptions);
            response.Cookies.Delete("RefreshToken", cookieOptions);
        }
    }

    public string? GetRefreshToken()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
    }

    private CookieOptions GetBaseCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };
    }
}