using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcoBridge.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace EcoBridge.IntegrationTests.Helpers;

public static class AuthTestHelper
{
    private const string SecretKey = "TestSuperSecretKeyForIntegrationTesting20261234567890!";
    private const string Issuer = "EcoBridgeAPI";
    private const string Audience = "EcoBridgeClients";

    public static string GenerateJwtToken(Guid userId, string email, UserRole role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role.ToString()),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
