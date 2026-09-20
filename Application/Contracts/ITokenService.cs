using System.Security.Claims;
using EcoBridge.Domain.Models.Accounts;

namespace EcoBridge.Application.Contracts;

public interface ITokenService
{
    Task<string> GenerateAccessToken(Account user, CancellationToken ct);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
