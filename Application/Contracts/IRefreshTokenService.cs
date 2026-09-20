namespace EcoBridge.Application.Contracts;

public interface IRefreshTokenService
{
    Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, CancellationToken ct = default);
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default);
}
