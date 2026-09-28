using EcoBridge.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Infrastructure.Services.Implementation;

public class CookieService : ICookieService
{
    // Single source of truth for cookie names — lowercase, used everywhere
    private const string RefreshTokenCookie = "refreshToken";
    private const string CsrfTokenCookie = "csrfToken";

    // Must match on Append AND Delete, or the browser won't remove the cookie
    private const string AuthPath = "/api/Auth";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetAuthCookies(string refreshToken, string csrfToken, DateTime refreshExpiresUtc)
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response == null) return;

        // Refresh token: HttpOnly, JS must NOT read it
        response.Cookies.Append(RefreshTokenCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = AuthPath,
            IsEssential = true,
            Expires = new DateTimeOffset(refreshExpiresUtc, TimeSpan.Zero)
        });

        // CSRF token: NOT HttpOnly, JS MUST read it (double-submit pattern)
        response.Cookies.Append(CsrfTokenCookie, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = AuthPath,
            IsEssential = true,
            Expires = new DateTimeOffset(refreshExpiresUtc, TimeSpan.Zero)
        });
    }

    public string? GetRefreshToken()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookie];
    }

    public string? GetCsrfToken()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[CsrfTokenCookie];
    }

    public void ClearAuthCookies()
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response == null) return;

        // Delete MUST use the same name AND same Path as Append
        var deleteOptions = new CookieOptions
        {
            Path = AuthPath,
            Secure = true,
            SameSite = SameSiteMode.None
        };

        response.Cookies.Delete(RefreshTokenCookie, deleteOptions);
        response.Cookies.Delete(CsrfTokenCookie, deleteOptions);
    }
}