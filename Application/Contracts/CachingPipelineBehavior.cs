using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.Contracts;

public class CachingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingPipelineBehavior<TRequest, TResponse>> _logger;

    public CachingPipelineBehavior(ICacheService cacheService, ILogger<CachingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Check if query implements ICacheableQuery
        if (request is ICacheableQuery cacheableQuery)
        {
            var cacheKey = cacheableQuery.CacheKey;
            var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse != null)
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", cacheKey);
                return cachedResponse;
            }

            _logger.LogDebug("Cache MISS for key: {CacheKey}", cacheKey);
            var response = await next();

            if (response != null)
            {
                var ttl = TimeSpan.FromMinutes(cacheableQuery.CacheDurationMinutes > 0 ? cacheableQuery.CacheDurationMinutes : 15);
                await _cacheService.SetAsync(cacheKey, response, ttl, cancellationToken);
            }

            return response;
        }

        // 2. Check if query class has [Cacheable] attribute
        var cacheableAttr = typeof(TRequest).GetCustomAttribute<CacheableAttribute>();
        if (cacheableAttr != null)
        {
            var prefix = cacheableAttr.CustomKeyPrefix ?? typeof(TRequest).Name;
            var cacheKey = $"{prefix}:{request.GetHashCode()}";

            var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse != null)
            {
                _logger.LogDebug("Cache HIT for key: {CacheKey}", cacheKey);
                return cachedResponse;
            }

            _logger.LogDebug("Cache MISS for key: {CacheKey}", cacheKey);
            var response = await next();

            if (response != null)
            {
                var ttl = TimeSpan.FromMinutes(cacheableAttr.TTLMinutes > 0 ? cacheableAttr.TTLMinutes : 15);
                await _cacheService.SetAsync(cacheKey, response, ttl, cancellationToken);
            }

            return response;
        }

        // Pass through if not cacheable
        return await next();
    }
}
