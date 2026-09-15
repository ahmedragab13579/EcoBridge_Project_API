using System.Text.Json;
using EcoBridge.Application.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EcoBridge.Infrastructure.Services.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisCacheService>? _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer? redis = null,
        ILogger<RedisCacheService>? logger = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var data = await _cache.GetStringAsync(key, ct);
            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error during GetAsync for key '{Key}'", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(15)
            };

            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _cache.SetStringAsync(key, json, options, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error during SetAsync for key '{Key}'", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error during RemoveAsync for key '{Key}'", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            if (_redis == null || !_redis.IsConnected)
                return;

            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                if (!server.IsReplica)
                {
                    var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                    foreach (var key in keys)
                    {
                        await _cache.RemoveAsync(key.ToString(), ct);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error during RemoveByPrefixAsync for prefix '{Prefix}'", prefix);
        }
    }
}
