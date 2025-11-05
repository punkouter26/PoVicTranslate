using System.Text.Json;
using System.Text.RegularExpressions;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Services.Caching;
using Po.VicTranslate.Api.Services.Lyrics;

namespace Po.VicTranslate.Api.Services;

public interface ILyricsManagementService
{
    Task<LyricsCollection> LoadLyricsCollectionAsync();
    Task<List<Song>> SearchLyricsAsync(string query, int maxResults = 10);
    Task<Song?> GetSongByIdAsync(string id);
    Task<List<string>> GetAvailableArtistsAsync();
    Task<List<string>> GetAvailableAlbumsAsync();
    Task RegenerateLyricsCollectionAsync();
}

public class LyricsManagementService : ILyricsManagementService
{
    private readonly ILogger<LyricsManagementService> _logger;
    private readonly ICacheService _cacheService;
    private readonly string _lyricsDataPath;
    private readonly string _scrapesPath;
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    public LyricsManagementService(
        ILogger<LyricsManagementService> logger, 
        IWebHostEnvironment environment,
        ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _logger = logger;
        _cacheService = cacheService;
        _lyricsDataPath = Path.Combine(environment.ContentRootPath, "Data", "lyrics-collection.json");
        _scrapesPath = Path.Combine(environment.ContentRootPath, "..", "scrapes");
    }

    public async Task<LyricsCollection> LoadLyricsCollectionAsync()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.LyricsCollection,
            async () =>
            {
                await _loadSemaphore.WaitAsync();
                try
                {
                    if (!File.Exists(_lyricsDataPath))
                    {
                        _logger.LogInformation("Lyrics collection not found, generating from text files...");
                        await RegenerateLyricsCollectionInternalAsync();
                    }

                    var jsonContent = await File.ReadAllTextAsync(_lyricsDataPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var collection = JsonSerializer.Deserialize<LyricsCollection>(jsonContent, options)
                        ?? throw new InvalidOperationException("Failed to deserialize lyrics collection");

                    _logger.LogInformation("Loaded lyrics collection with {SongCount} songs", collection.TotalSongs);
                    return collection;
                }
                finally
                {
                    _loadSemaphore.Release();
                }
            },
            absoluteExpiration: TimeSpan.FromHours(24)); // Cache for 24 hours
    }

    public async Task<List<Song>> SearchLyricsAsync(string query, int maxResults = 10)
    {
        var collection = await LoadLyricsCollectionAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return collection.Songs.Take(maxResults).ToList();
        }

        var queryLower = query.ToLowerInvariant();
        var queryTerms = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var results = collection.Songs
            .Select(song => new LyricsSearchResult
            {
                Song = song,
                RelevanceScore = CalculateRelevanceScore(song, queryLower, queryTerms),
                MatchedTerms = GetMatchedTerms(song, queryTerms)
            })
            .Where(r => r.RelevanceScore > 0)
            .OrderByDescending(r => r.RelevanceScore)
            .Take(maxResults)
            .Select(r => r.Song)
            .ToList();

        _logger.LogInformation("Search for '{Query}' returned {ResultCount} results", query, results.Count);
        return results;
    }

    public async Task<Song?> GetSongByIdAsync(string id)
    {
        var cacheKey = CacheKeys.GetSongKey(id);
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var collection = await LoadLyricsCollectionAsync();
                return collection.Songs.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            },
            absoluteExpiration: TimeSpan.FromHours(24));
    }

    public async Task<List<string>> GetAvailableArtistsAsync()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.AvailableArtists,
            async () =>
            {
                var collection = await LoadLyricsCollectionAsync();
                return collection.Artists.Values.Distinct().OrderBy(x => x).ToList();
            },
            absoluteExpiration: TimeSpan.FromHours(24));
    }

    public async Task<List<string>> GetAvailableAlbumsAsync()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.AvailableAlbums,
            async () =>
            {
                var collection = await LoadLyricsCollectionAsync();
                return collection.Albums.Values.Distinct().OrderBy(x => x).ToList();
            },
            absoluteExpiration: TimeSpan.FromHours(24));
    }

    public async Task RegenerateLyricsCollectionAsync()
    {
        await RegenerateLyricsCollectionInternalAsync();
        
        // Invalidate all lyrics-related cache entries
        _cacheService.RemoveByPrefix("lyrics:");
        _logger.LogInformation("Invalidated all lyrics cache entries after regeneration");
    }

    private async Task RegenerateLyricsCollectionInternalAsync()
    {
        _logger.LogInformation("Starting lyrics collection regeneration...");

        if (!Directory.Exists(_scrapesPath))
        {
            throw new DirectoryNotFoundException($"Scrapes directory not found: {_scrapesPath}");
        }

        var textFiles = Directory.GetFiles(_scrapesPath, "*.txt");
        var songs = new List<Song>();
        var artists = new Dictionary<string, string>();
        var albums = new Dictionary<string, string>();

        foreach (var filePath in textFiles)
        {
            try
            {
                var song = await ProcessLyricsFileAsync(filePath);
                songs.Add(song);

                // Build artist and album dictionaries
                if (!artists.ContainsKey(song.Artist))
                {
                    artists[song.Artist] = song.Artist;
                }

                if (!albums.ContainsKey(song.Album))
                {
                    albums[song.Album] = song.Album;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process lyrics file: {FilePath}", filePath);
            }
        }

        var collection = new LyricsCollection
        {
            Version = "1.0",
            GeneratedAt = DateTime.UtcNow,
            TotalSongs = songs.Count,
            Artists = artists,
            Albums = albums,
            Songs = songs.OrderBy(s => s.Title).ToList()
        };

        // Ensure Data directory exists
        var dataDir = Path.GetDirectoryName(_lyricsDataPath)!;
        Directory.CreateDirectory(dataDir);

        // Save to JSON with pretty formatting
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonContent = JsonSerializer.Serialize(collection, options);
        await File.WriteAllTextAsync(_lyricsDataPath, jsonContent);

        _logger.LogInformation("Generated lyrics collection with {SongCount} songs", collection.TotalSongs);
    }

    private async Task<Song> ProcessLyricsFileAsync(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var content = await File.ReadAllTextAsync(filePath);

        // Clean up content
        content = content.Trim();
        var wordCount = CountWords(content);

        // Parse metadata from filename and content
        var (title, artist, album) = ParseSongMetadata(fileName, content);

        return new Song
        {
            Id = fileName.ToLowerInvariant(),
            Title = title,
            Artist = artist,
            Album = album,
            Genre = "Hip-Hop", // Default for Wu-Tang collection
            Content = content,
            WordCount = wordCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tags = GenerateTags(fileName, content),
            Description = GenerateDescription(title, artist, wordCount)
        };
    }

    private static (string title, string artist, string album) ParseSongMetadata(string fileName, string content)
    {
        // Default values
        var title = FormatTitle(fileName);
        var artist = "Wu-Tang Clan";
        var album = "Various";

        // Try to detect specific Wu-Tang members from content or filename
        var wuTangMembers = new Dictionary<string, string>
        {
            { "method", "Method Man" },
            { "raekwon", "Raekwon" },
            { "ghostface", "Ghostface Killah" },
            { "odb", "Ol' Dirty Bastard" },
            { "rza", "RZA" },
            { "gza", "GZA" },
            { "inspectah", "Inspectah Deck" },
            { "ugodk", "U-God" },
            { "masta", "Masta Killa" },
            { "cappadonna", "Cappadonna" }
        };

        // Check filename for artist hints
        var fileNameLower = fileName.ToLowerInvariant();
        foreach (var (key, member) in wuTangMembers)
        {
            if (fileNameLower.Contains(key))
            {
                artist = member;
                break;
            }
        }

        // Detect album from common Wu-Tang album patterns
        if (fileNameLower.Contains("36chamber") || fileNameLower.Contains("shaolin"))
        {
            album = "Enter the Wu-Tang (36 Chambers)";
        }
        else if (fileNameLower.Contains("wuforever"))
        {
            album = "Wu-Tang Forever";
        }
        else if (fileNameLower.Contains("iron") && fileNameLower.Contains("flag"))
        {
            album = "Iron Flag";
        }

        return (title, artist, album);
    }

    private static string FormatTitle(string fileName)
    {
        // Convert filename to proper title case
        var title = fileName.Replace("_", " ").Replace("-", " ");

        // Handle common Wu-Tang abbreviations
        title = Regex.Replace(title, @"\bwu\b", "Wu", RegexOptions.IgnoreCase);
        title = Regex.Replace(title, @"\btang\b", "Tang", RegexOptions.IgnoreCase);
        title = Regex.Replace(title, @"\bda\b", "Da", RegexOptions.IgnoreCase);
        title = Regex.Replace(title, @"\bthe\b", "The", RegexOptions.IgnoreCase);

        // Convert to title case
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLowerInvariant());
    }

    /// <summary>
    /// Generates tags for a song using the Builder pattern.
    /// Complexity reduced from 16 to ~4 by extracting tag generation logic into composable steps.
    /// </summary>
    private static List<string> GenerateTags(string fileName, string content)
    {
        return new SongTagsBuilder()
            .WithFileName(fileName)
            .WithContent(content)
            .AddBaseTags()
            .AddMartialArtsTags()
            .AddAlbumTags()
            .AddFormatTags()
            .AddClassicTags()
            .Build();
    }

    private static string GenerateDescription(string title, string artist, int wordCount)
    {
        return $"{title} by {artist} - {wordCount} words";
    }

    private static int CountWords(string content)
    {
        return content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static double CalculateRelevanceScore(Song song, string queryLower, string[] queryTerms)
    {
        double score = 0;

        // Title matches (highest weight)
        if (song.Title.ToLowerInvariant().Contains(queryLower))
        {
            score += 10;
        }

        // Artist matches
        if (song.Artist.ToLowerInvariant().Contains(queryLower))
        {
            score += 8;
        }

        // Content matches (lower weight due to length)
        var contentLower = song.Content.ToLowerInvariant();
        foreach (var term in queryTerms)
        {
            var matches = Regex.Matches(contentLower, Regex.Escape(term), RegexOptions.IgnoreCase).Count;
            score += matches * 0.5;
        }

        // Tag matches
        foreach (var tag in song.Tags)
        {
            if (tag.ToLowerInvariant().Contains(queryLower))
            {
                score += 3;
            }
        }

        return score;
    }

    private static List<string> GetMatchedTerms(Song song, string[] queryTerms)
    {
        var matched = new List<string>();
        var searchText = $"{song.Title} {song.Artist} {song.Content} {string.Join(" ", song.Tags)}".ToLowerInvariant();

        foreach (var term in queryTerms)
        {
            if (searchText.Contains(term))
            {
                matched.Add(term);
            }
        }

        return matched;
    }
}
