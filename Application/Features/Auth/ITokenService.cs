using System.Security.Claims;
using EcoBridge.Domain.Models;

namespace EcoBridge.Application.Features.Auth;

public interface ITokenService
{
    Task<string> GenerateAccessToken(Account user, CancellationToken ct);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
