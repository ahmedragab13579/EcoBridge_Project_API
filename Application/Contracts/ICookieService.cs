namespace EcoBridge.Application.Contracts;

public interface ICookieService
{
    /// <summary>
    /// Sets the refresh-token cookie (HttpOnly) and the CSRF cookie (JS-readable).
    /// Call this on login, register, and refresh.
    /// </summary>
    void SetAuthCookies(string refreshToken, string csrfToken, DateTime refreshExpiresUtc);

    /// <summary>
    /// Reads the refresh token from the request cookie. Returns null if absent.
    /// </summary>
    string? GetRefreshToken();

    /// <summary>
    /// Reads the CSRF token from the request cookie. Returns null if absent.
    /// </summary>
    string? GetCsrfToken();

    /// <summary>
    /// Deletes both auth cookies. Call this on logout/revoke.
    /// </summary>
    void ClearAuthCookies();
}