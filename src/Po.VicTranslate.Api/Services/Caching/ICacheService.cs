namespace Po.VicTranslate.Api.Services.Caching;

/// <summary>
/// Service for managing application-level caching operations
/// Provides a simplified interface over IMemoryCache with telemetry
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value or creates it using the factory function
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Removes a specific cache entry
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Removes all cache entries matching a prefix pattern
    /// </summary>
    void RemoveByPrefix(string prefix);

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets cache statistics for monitoring
    /// </summary>
    CacheStatistics GetStatistics();
}

/// <summary>
/// Statistics about cache performance
/// </summary>
public class CacheStatistics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public long TotalEvictions { get; set; }
    public double HitRate => TotalHits + TotalMisses > 0
        ? (double)TotalHits / (TotalHits + TotalMisses) * 100
        : 0;
}
