using System.Text.Json;
using System.Text.RegularExpressions;
using VictorianTranslator.Server.Models;

namespace VictorianTranslator.Server.Services;

public interface ILyricsManagementService
{
    Task<LyricsCollection> LoadLyricsCollectionAsync();
    Task<List<LyricsSong>> SearchLyricsAsync(string query, int maxResults = 10);
    Task<LyricsSong?> GetSongByIdAsync(string id);
    Task<List<string>> GetAvailableArtistsAsync();
    Task<List<string>> GetAvailableAlbumsAsync();
    Task RegenerateLyricsCollectionAsync();
}

public class LyricsManagementService : ILyricsManagementService
{
    private readonly ILogger<LyricsManagementService> _logger;
    private readonly string _lyricsDataPath;
    private readonly string _scrapesPath;
    private LyricsCollection? _cachedCollection;
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

    public LyricsManagementService(ILogger<LyricsManagementService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _lyricsDataPath = Path.Combine(environment.ContentRootPath, "Data", "lyrics-collection.json");
        _scrapesPath = Path.Combine(environment.ContentRootPath, "..", "scrapes");
    }

    public async Task<LyricsCollection> LoadLyricsCollectionAsync()
    {
        await _loadSemaphore.WaitAsync();
        try
        {
            if (_cachedCollection != null)
                return _cachedCollection;

            if (!File.Exists(_lyricsDataPath))
            {
                _logger.LogInformation("Lyrics collection not found, generating from text files...");
                await RegenerateLyricsCollectionAsync();
            }

            var jsonContent = await File.ReadAllTextAsync(_lyricsDataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _cachedCollection = JsonSerializer.Deserialize<LyricsCollection>(jsonContent, options)
                ?? throw new InvalidOperationException("Failed to deserialize lyrics collection");

            _logger.LogInformation("Loaded lyrics collection with {SongCount} songs", _cachedCollection.TotalSongs);
            return _cachedCollection;
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }

    public async Task<List<LyricsSong>> SearchLyricsAsync(string query, int maxResults = 10)
    {
        var collection = await LoadLyricsCollectionAsync();

        if (string.IsNullOrWhiteSpace(query))
            return collection.Songs.Take(maxResults).ToList();

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

    public async Task<LyricsSong?> GetSongByIdAsync(string id)
    {
        var collection = await LoadLyricsCollectionAsync();
        return collection.Songs.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<string>> GetAvailableArtistsAsync()
    {
        var collection = await LoadLyricsCollectionAsync();
        return collection.Artists.Values.Distinct().OrderBy(x => x).ToList();
    }

    public async Task<List<string>> GetAvailableAlbumsAsync()
    {
        var collection = await LoadLyricsCollectionAsync();
        return collection.Albums.Values.Distinct().OrderBy(x => x).ToList();
    }

    public async Task RegenerateLyricsCollectionAsync()
    {
        _logger.LogInformation("Starting lyrics collection regeneration...");

        if (!Directory.Exists(_scrapesPath))
            throw new DirectoryNotFoundException($"Scrapes directory not found: {_scrapesPath}");

        var textFiles = Directory.GetFiles(_scrapesPath, "*.txt");
        var songs = new List<LyricsSong>();
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
                    artists[song.Artist] = song.Artist;
                if (!albums.ContainsKey(song.Album))
                    albums[song.Album] = song.Album;
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

        _cachedCollection = collection;
        _logger.LogInformation("Generated lyrics collection with {SongCount} songs", collection.TotalSongs);
    }

    private async Task<LyricsSong> ProcessLyricsFileAsync(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var content = await File.ReadAllTextAsync(filePath);

        // Clean up content
        content = content.Trim();
        var wordCount = CountWords(content);

        // Parse metadata from filename and content
        var (title, artist, album) = ParseSongMetadata(fileName, content);

        return new LyricsSong
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
            album = "Enter the Wu-Tang (36 Chambers)";
        else if (fileNameLower.Contains("wuforever"))
            album = "Wu-Tang Forever";
        else if (fileNameLower.Contains("iron") && fileNameLower.Contains("flag"))
            album = "Iron Flag";

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

    private static List<string> GenerateTags(string fileName, string content)
    {
        var tags = new List<string> { "hip-hop", "wu-tang" };

        var fileNameLower = fileName.ToLowerInvariant();
        var contentLower = content.ToLowerInvariant();

        // Add tags based on content analysis
        if (contentLower.Contains("shaolin") || contentLower.Contains("kung fu"))
            tags.Add("martial-arts");
        if (contentLower.Contains("chamber"))
            tags.Add("36-chambers");
        if (fileNameLower.Contains("remix"))
            tags.Add("remix");
        if (fileNameLower.Contains("intro") || fileNameLower.Contains("outro"))
            tags.Add("interlude");
        if (contentLower.Contains("cream") || contentLower.Contains("cash rules"))
            tags.Add("classic");

        return tags.Distinct().ToList();
    }

    private static string GenerateDescription(string title, string artist, int wordCount)
    {
        return $"{title} by {artist} - {wordCount} words";
    }

    private static int CountWords(string content)
    {
        return content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static double CalculateRelevanceScore(LyricsSong song, string queryLower, string[] queryTerms)
    {
        double score = 0;

        // Title matches (highest weight)
        if (song.Title.ToLowerInvariant().Contains(queryLower))
            score += 10;

        // Artist matches
        if (song.Artist.ToLowerInvariant().Contains(queryLower))
            score += 8;

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
                score += 3;
        }

        return score;
    }

    private static List<string> GetMatchedTerms(LyricsSong song, string[] queryTerms)
    {
        var matched = new List<string>();
        var searchText = $"{song.Title} {song.Artist} {song.Content} {string.Join(" ", song.Tags)}".ToLowerInvariant();

        foreach (var term in queryTerms)
        {
            if (searchText.Contains(term))
                matched.Add(term);
        }

        return matched;
    }
}
