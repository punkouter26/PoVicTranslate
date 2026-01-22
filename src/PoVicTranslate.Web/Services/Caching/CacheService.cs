using Microsoft.Extensions.Caching.Memory;

namespace PoVicTranslate.Web.Services.Caching;

/// <summary>
/// Memory-based caching service.
/// </summary>
public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        var value = await factory();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration ?? DefaultExpiration);

        _cache.Set(key, value, cacheOptions);

        return value;
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Removed cache entry: {Key}", key);
    }
}
