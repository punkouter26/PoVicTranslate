namespace Po.VicTranslate.Api.Services.Caching;

/// <summary>
/// Centralized cache key management for the application
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Cache key for the entire lyrics collection
    /// </summary>
    public const string LyricsCollection = "lyrics:collection";

    /// <summary>
    /// Cache key prefix for individual songs by ID
    /// </summary>
    public const string SongByIdPrefix = "lyrics:song:";

    /// <summary>
    /// Cache key for available artists list
    /// </summary>
    public const string AvailableArtists = "lyrics:artists";

    /// <summary>
    /// Cache key for available albums list
    /// </summary>
    public const string AvailableAlbums = "lyrics:albums";

    /// <summary>
    /// Gets a cache key for a specific song ID
    /// </summary>
    public static string GetSongKey(string songId) => $"{SongByIdPrefix}{songId}";
}
