using System.Security.Cryptography;
using System.Text.Json;
using EcoBridge.Application.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoBridge.Application.Services;

public class RedisRefreshTokenService : IRefreshTokenService
{
    private readonly IDistributedCache _cache;
    private readonly JWTSettings _jwtSettings;
    private readonly ILogger<RedisRefreshTokenService> _logger;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public RedisRefreshTokenService(
        IDistributedCache cache,
        IOptions<JWTSettings> jwtOptions,
        ILogger<RedisRefreshTokenService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToBase64String(tokenBytes);

        var days = _jwtSettings.RefreshTokenDurationInDays > 0 ? _jwtSettings.RefreshTokenDurationInDays : 30;
        var expiry = TimeSpan.FromDays(days);

        var tokenKey = GetTokenKey(token);
        var userTokensKey = GetUserTokensKey(userId);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };

        try
        {
            // 1. Save token -> userId mapping
            await _cache.SetStringAsync(tokenKey, userId.ToString(), options, ct);

            // 2. Concurrency-safe update of user's active tokens list
            await _lock.WaitAsync(ct);
            try
            {
                var userTokens = await GetUserActiveTokensAsync(userId, ct);
                if (!userTokens.Contains(token))
                {
                    userTokens.Add(token);
                }
                await _cache.SetStringAsync(userTokensKey, JsonSerializer.Serialize(userTokens), options, ct);
            }
            finally
            {
                _lock.Release();
            }

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist refresh token for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var tokenKey = GetTokenKey(refreshToken);

        try
        {
            var userIdStr = await _cache.GetStringAsync(tokenKey, ct);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return null;

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate refresh token against cache");
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var tokenKey = GetTokenKey(refreshToken);

        try
        {
            var userIdStr = await _cache.GetStringAsync(tokenKey, ct);
            await _cache.RemoveAsync(tokenKey, ct);

            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
            {
                await _lock.WaitAsync(ct);
                try
                {
                    var userTokens = await GetUserActiveTokensAsync(userId, ct);
                    if (userTokens.Remove(refreshToken))
                    {
                        var userTokensKey = GetUserTokensKey(userId);
                        if (userTokens.Count > 0)
                        {
                            await _cache.SetStringAsync(userTokensKey, JsonSerializer.Serialize(userTokens), ct);
                        }
                        else
                        {
                            await _cache.RemoveAsync(userTokensKey, ct);
                        }
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke refresh token");
            return false;
        }
    }

    public async Task<bool> RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var userTokensKey = GetUserTokensKey(userId);

        try
        {
            await _lock.WaitAsync(ct);
            try
            {
                var userTokens = await GetUserActiveTokensAsync(userId, ct);
                foreach (var token in userTokens)
                {
                    await _cache.RemoveAsync(GetTokenKey(token), ct);
                }

                await _cache.RemoveAsync(userTokensKey, ct);
            }
            finally
            {
                _lock.Release();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke all refresh tokens for user {UserId}", userId);
            return false;
        }
    }

    private async Task<List<string>> GetUserActiveTokensAsync(Guid userId, CancellationToken ct)
    {
        var userTokensKey = GetUserTokensKey(userId);
        var json = await _cache.GetStringAsync(userTokensKey, ct);
        if (string.IsNullOrEmpty(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static string GetTokenKey(string token) => $"refresh_token:{token}";
    private static string GetUserTokensKey(Guid userId) => $"user_tokens:{userId}";
}
