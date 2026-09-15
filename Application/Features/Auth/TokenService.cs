using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EcoBridge.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcoBridge.Application.Features.Auth;

public class TokenService : ITokenService
{
    private readonly JWTSettings _jwtSettings;

    public TokenService(IOptions<JWTSettings> jwtOptions)
    {
        if (jwtOptions == null) throw new ArgumentNullException(nameof(jwtOptions));
        _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));

        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured. Set JWT:SecretKey in configuration.");
        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer) || string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            throw new InvalidOperationException("JWT Issuer/Audience are not configured. Set JWT:Issuer and JWT:Audience in configuration.");
        if (_jwtSettings.DurationInMinutes <= 0)
            throw new InvalidOperationException("JWT DurationInMinutes must be greater than zero.");
    }

    public Task<string> GenerateAccessToken(Account user, CancellationToken ct)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.RoleId.ToString())
        };

        if (user.CityId.HasValue)
        {
            claims.Add(new Claim("cityId", user.CityId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
