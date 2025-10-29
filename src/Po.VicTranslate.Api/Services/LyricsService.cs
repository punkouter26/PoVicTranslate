using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services;

public class LyricsService : ILyricsService
{
    private readonly string _lyricsFilePath;
    private const int MaxWords = 200;
    private LyricsCollection? _lyricsCache;
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    public LyricsService(IWebHostEnvironment webHostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(webHostEnvironment);
        _lyricsFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Data", "lyrics-collection.json");
    }

    private async Task<LyricsCollection> GetLyricsCollectionAsync()
    {
        if (_lyricsCache != null)
        {
            return _lyricsCache;
        }

        await _loadSemaphore.WaitAsync();
        try
        {
            // Double-check pattern for thread-safe lazy initialization
#pragma warning disable CA1508 // Avoid dead conditional code - false positive, this is intentional double-check locking
            if (_lyricsCache != null)
            {
                return _lyricsCache;
            }
#pragma warning restore CA1508

            if (!File.Exists(_lyricsFilePath))
            {
                throw new FileNotFoundException($"Lyrics collection file not found: {_lyricsFilePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(_lyricsFilePath);
            _lyricsCache = JsonSerializer.Deserialize<LyricsCollection>(jsonContent);

            if (_lyricsCache == null)
            {
                throw new InvalidOperationException("Failed to deserialize lyrics collection");
            }

            return _lyricsCache;
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }

    public async Task<List<string>> GetAvailableSongsAsync()
    {
        var collection = await GetLyricsCollectionAsync();
        return collection.Songs
            .Select(s => $"{s.Id}.json") // Return as .json to maintain API compatibility
            .OrderBy(f => f)
            .ToList();
    }

    public async Task<string> GetLyricsAsync(string songFileName)
    {
        // Remove .json or .txt extension if present to get the song ID
        var songId = Path.GetFileNameWithoutExtension(songFileName);

        var collection = await GetLyricsCollectionAsync();
        var song = collection.Songs.FirstOrDefault(s => s.Id.Equals(songId, StringComparison.OrdinalIgnoreCase));

        if (song == null)
        {
            throw new FileNotFoundException($"Song not found: {songId}");
        }

        var fullText = song.Content;
        var words = fullText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= MaxWords)
        {
            return fullText;
        }

        return string.Join(" ", words.Take(MaxWords)) + "...";
    }
}
