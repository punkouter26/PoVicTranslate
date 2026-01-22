namespace PoVicTranslate.Web.Services.Caching;

/// <summary>
/// Interface for caching service.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value or creates it using the factory.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a cached value.
    /// </summary>
    void Remove(string key);
}
