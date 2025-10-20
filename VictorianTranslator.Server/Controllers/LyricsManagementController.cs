using Microsoft.AspNetCore.Mvc;
using VictorianTranslator.Server.Services;
using VictorianTranslator.Server.Models;

namespace VictorianTranslator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LyricsManagementController : ControllerBase
{
    private readonly ILyricsManagementService _lyricsManagement;
    private readonly ILogger<LyricsManagementController> _logger;

    public LyricsManagementController(
        ILyricsManagementService lyricsManagement,
        ILogger<LyricsManagementController> logger)
    {
        _lyricsManagement = lyricsManagement;
        _logger = logger;
    }

    [HttpGet("collection")]
    public async Task<ActionResult<LyricsCollection>> GetLyricsCollection()
    {
        try
        {
            var collection = await _lyricsManagement.LoadLyricsCollectionAsync();
            return Ok(collection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load lyrics collection");
            return StatusCode(500, "Failed to load lyrics collection");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<LyricsSong>>> SearchLyrics(
        [FromQuery] string? query = null,
        [FromQuery] int maxResults = 10)
    {
        try
        {
            var results = await _lyricsManagement.SearchLyricsAsync(query ?? "", maxResults);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search lyrics for query: {Query}", query);
            return StatusCode(500, "Failed to search lyrics");
        }
    }

    [HttpGet("song/{id}")]
    public async Task<ActionResult<LyricsSong>> GetSong(string id)
    {
        try
        {
            var song = await _lyricsManagement.GetSongByIdAsync(id);
            if (song == null)
                return NotFound($"Song with ID '{id}' not found");

            return Ok(song);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get song by ID: {Id}", id);
            return StatusCode(500, "Failed to get song");
        }
    }

    [HttpGet("artists")]
    public async Task<ActionResult<List<string>>> GetArtists()
    {
        try
        {
            var artists = await _lyricsManagement.GetAvailableArtistsAsync();
            return Ok(artists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available artists");
            return StatusCode(500, "Failed to get artists");
        }
    }

    [HttpGet("albums")]
    public async Task<ActionResult<List<string>>> GetAlbums()
    {
        try
        {
            var albums = await _lyricsManagement.GetAvailableAlbumsAsync();
            return Ok(albums);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available albums");
            return StatusCode(500, "Failed to get albums");
        }
    }

    [HttpPost("regenerate")]
    public async Task<ActionResult> RegenerateCollection()
    {
        try
        {
            await _lyricsManagement.RegenerateLyricsCollectionAsync();
            return Ok("Lyrics collection regenerated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to regenerate lyrics collection");
            return StatusCode(500, "Failed to regenerate lyrics collection");
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetCollectionStats()
    {
        try
        {
            var collection = await _lyricsManagement.LoadLyricsCollectionAsync();

            var stats = new
            {
                TotalSongs = collection.TotalSongs,
                TotalArtists = collection.Artists.Count,
                TotalAlbums = collection.Albums.Count,
                GeneratedAt = collection.GeneratedAt,
                Version = collection.Version,
                TotalWords = collection.Songs.Sum(s => s.WordCount),
                AverageWordsPerSong = collection.Songs.Count > 0 ? collection.Songs.Average(s => s.WordCount) : 0,
                TopArtists = collection.Songs
                    .GroupBy(s => s.Artist)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new { Artist = g.Key, SongCount = g.Count() })
                    .ToList(),
                TopTags = collection.Songs
                    .SelectMany(s => s.Tags)
                    .GroupBy(t => t)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => new { Tag = g.Key, Count = g.Count() })
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection stats");
            return StatusCode(500, "Failed to get collection stats");
        }
    }
}
