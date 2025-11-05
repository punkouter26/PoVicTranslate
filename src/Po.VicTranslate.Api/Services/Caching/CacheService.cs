using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Po.VicTranslate.Api.Services.Caching;

/// <summary>
/// Implementation of cache service with telemetry and prefix-based eviction
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly ICustomTelemetryService? _telemetryService;
    private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();
    private long _hits;
    private long _misses;
    private long _evictions;

    public CacheService(
        IMemoryCache cache, 
        ILogger<CacheService> logger,
        ICustomTelemetryService? telemetryService = null)
    {
        _cache = cache;
        _logger = logger;
        _telemetryService = telemetryService;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null)
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
        {
            Interlocked.Increment(ref _hits);
            _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
            
            // Track cache hit telemetry
            _telemetryService?.TrackCacheHit(key, GetCacheType(key));
            
            return cachedValue!;
        }

        Interlocked.Increment(ref _misses);
        _logger.LogDebug("Cache MISS for key: {CacheKey}", key);
        
        // Track cache miss telemetry
        _telemetryService?.TrackCacheMiss(key, GetCacheType(key));

        var value = await factory();

        var cacheEntryOptions = new MemoryCacheEntryOptions();
        
        if (absoluteExpiration.HasValue)
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
        }
        else
        {
            // Default: 1 hour sliding expiration
            cacheEntryOptions.SlidingExpiration = TimeSpan.FromHours(1);
        }

        // Register eviction callback with telemetry
        cacheEntryOptions.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
        {
            Interlocked.Increment(ref _evictions);
            _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
            _logger.LogDebug("Cache entry evicted: {CacheKey}, Reason: {Reason}", evictedKey, reason);
            
            // Track eviction telemetry
            _telemetryService?.TrackCacheEviction(
                evictedKey.ToString()!, 
                GetCacheType(evictedKey.ToString()!), 
                reason.ToString());
        });

        _cache.Set(key, value, cacheEntryOptions);
        _cacheKeys.TryAdd(key, 0);
        
        _logger.LogInformation("Cached new entry with key: {CacheKey}", key);
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
        _logger.LogInformation("Removed cache entry: {CacheKey}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();
        
        foreach (var key in keysToRemove)
        {
            Remove(key);
        }

        _logger.LogInformation("Removed {Count} cache entries with prefix: {Prefix}", keysToRemove.Count, prefix);
    }

    public void Clear()
    {
        var allKeys = _cacheKeys.Keys.ToList();
        
        foreach (var key in allKeys)
        {
            _cache.Remove(key);
        }

        _cacheKeys.Clear();
        _logger.LogWarning("Cleared all cache entries ({Count} entries)", allKeys.Count);
    }

    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            TotalHits = Interlocked.Read(ref _hits),
            TotalMisses = Interlocked.Read(ref _misses),
            TotalEvictions = Interlocked.Read(ref _evictions)
        };
    }

    private static string GetCacheType(string key)
    {
        if (key.StartsWith("lyrics:", StringComparison.OrdinalIgnoreCase))
            return "Lyrics";
        if (key.StartsWith("song:", StringComparison.OrdinalIgnoreCase))
            return "Song";
        if (key.StartsWith("artist:", StringComparison.OrdinalIgnoreCase))
            return "Artist";
        if (key.StartsWith("album:", StringComparison.OrdinalIgnoreCase))
            return "Album";
        
        return "Other";
    }
}
