using System.Text.Json;
using System.Text.Json.Serialization;
using EcoBridge.Application.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EcoBridge.Infrastructure.Services.Implementation;

public class RedisServices : IRedisServices
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisServices>? _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public RedisServices(
        IDistributedCache cache,
        IConnectionMultiplexer? redis = null,
        ILogger<RedisServices>? logger = null)
    {
        _cache = cache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetDataAsync<T>(string key)
    {
        try
        {
            var data = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data, SerializerOptions);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error during GetDataAsync for key '{Key}'", key);
            return default;
        }
    }

    public async Task SetDataAsync<T>(string key, T data, TimeSpan? expirationTime = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromMinutes(10)
            };

            var serializedData = JsonSerializer.Serialize(data, SerializerOptions);
            await _cache.SetStringAsync(key, serializedData, options);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error during SetDataAsync for key '{Key}'", key);
        }
    }

    public async Task RemoveDataAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error during RemoveDataAsync for key '{Key}'", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
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
                        await _cache.RemoveAsync(key.ToString());
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