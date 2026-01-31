using System.Text.Json.Serialization;

namespace PoVicTranslate.Web.Models;

/// <summary>
/// Collection of song lyrics.
/// </summary>
public sealed class LyricsCollection
{
    public List<SongEntry> Songs { get; set; } = [];
}

/// <summary>
/// Individual song entry with title and lyrics.
/// </summary>
public sealed class SongEntry
{
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Lyrics { get; set; } = string.Empty;
}
