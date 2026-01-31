namespace PoVicTranslate.Web.Services;

/// <summary>
/// Interface for lyrics service.
/// </summary>
public interface ILyricsService
{
    /// <summary>
    /// Gets the list of available song files.
    /// </summary>
    Task<List<string>> GetAvailableSongsAsync();

    /// <summary>
    /// Gets lyrics for a specific song, limited to 200 words.
    /// </summary>
    Task<string?> GetLyricsAsync(string songTitle);

    /// <summary>
    /// Gets random lyrics from the collection.
    /// </summary>
    Task<(string? Title, string? Lyrics)> GetRandomLyricsAsync();
}
