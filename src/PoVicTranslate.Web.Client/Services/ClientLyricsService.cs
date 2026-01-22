using System.Net.Http.Json;

namespace PoVicTranslate.Web.Client.Services;

/// <summary>
/// Client service for communicating with the server's lyrics endpoints.
/// </summary>
public sealed class ClientLyricsService(HttpClient httpClient)
{
    /// <summary>
    /// Gets all available song titles.
    /// </summary>
    public async Task<List<string>> GetSongTitlesAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<LyricsTitlesResponse>("/api/lyrics/titles", cancellationToken);
        return response?.Titles ?? [];
    }

    /// <summary>
    /// Gets lyrics for a specific song by title.
    /// </summary>
    public async Task<string?> GetLyricsAsync(string title, CancellationToken cancellationToken = default)
    {
        var encoded = Uri.EscapeDataString(title);
        var response = await httpClient.GetFromJsonAsync<LyricsResponse>($"/api/lyrics/{encoded}", cancellationToken);
        return response?.Lyrics;
    }

    /// <summary>
    /// Gets random lyrics from the collection.
    /// </summary>
    public async Task<string?> GetRandomLyricsAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<LyricsResponse>("/api/lyrics/random", cancellationToken);
        return response?.Lyrics;
    }
}

/// <summary>
/// Response DTO for lyrics titles endpoint.
/// </summary>
public sealed record LyricsTitlesResponse(List<string> Titles);

/// <summary>
/// Response DTO for lyrics content endpoint.
/// </summary>
public sealed record LyricsResponse(string Title, string Lyrics);
