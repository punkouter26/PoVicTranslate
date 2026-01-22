using System.Text.Json;
using PoVicTranslate.Web.Models;

namespace PoVicTranslate.Web.Services;

/// <summary>
/// Service for retrieving Victorian-era song lyrics.
/// </summary>
public sealed class LyricsService : ILyricsService
{
    private readonly string _lyricsFilePath;
    private readonly ILogger<LyricsService> _logger;
    private const int MaxWords = 200;
    private LyricsCollection? _lyricsCache;
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    public LyricsService(IWebHostEnvironment webHostEnvironment, ILogger<LyricsService> logger)
    {
        ArgumentNullException.ThrowIfNull(webHostEnvironment);
        _logger = logger;
        _lyricsFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Data", "lyrics-collection.json");
    }

    private async Task<LyricsCollection> GetLyricsCollectionAsync()
    {
        if (_lyricsCache is not null)
        {
            return _lyricsCache;
        }

        await _loadSemaphore.WaitAsync();
        try
        {
            if (_lyricsCache is not null)
            {
                return _lyricsCache;
            }

            if (!File.Exists(_lyricsFilePath))
            {
                _logger.LogError("Lyrics collection file not found: {Path}", _lyricsFilePath);
                throw new FileNotFoundException($"Lyrics collection file not found: {_lyricsFilePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(_lyricsFilePath);
            _lyricsCache = JsonSerializer.Deserialize<LyricsCollection>(jsonContent);

            if (_lyricsCache is null)
            {
                throw new InvalidOperationException("Failed to deserialize lyrics collection");
            }

            _logger.LogInformation("Loaded {Count} songs from lyrics collection", _lyricsCache.Songs.Count);
            return _lyricsCache;
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> GetAvailableSongsAsync()
    {
        var collection = await GetLyricsCollectionAsync();
        return collection.Songs
            .Select(s => s.Title)
            .OrderBy(f => f)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<string?> GetLyricsAsync(string songFileName)
    {
        var songId = Path.GetFileNameWithoutExtension(songFileName);

        var collection = await GetLyricsCollectionAsync();
        var song = collection.Songs.FirstOrDefault(s =>
            s.Title.Equals(songId, StringComparison.OrdinalIgnoreCase));

        if (song is null)
        {
            _logger.LogWarning("Song not found: {SongId}", songId);
            return null;
        }

        var fullText = song.Lyrics;
        var words = fullText.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= MaxWords)
        {
            return fullText;
        }

        return string.Join(" ", words.Take(MaxWords)) + "...";
    }

    /// <inheritdoc />
    public async Task<(string? Title, string? Lyrics)> GetRandomLyricsAsync()
    {
        var collection = await GetLyricsCollectionAsync();
        if (collection.Songs.Count == 0)
        {
            return (null, null);
        }

        var random = new Random();
        var song = collection.Songs[random.Next(collection.Songs.Count)];
        var lyrics = await GetLyricsAsync(song.Title);
        return (song.Title, lyrics);
    }
}
