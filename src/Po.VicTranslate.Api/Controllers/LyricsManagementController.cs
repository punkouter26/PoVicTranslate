using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Controllers.Base;
using Po.VicTranslate.Api.Services.Validation;

namespace Po.VicTranslate.Api.Controllers;

/// <summary>
/// Controller for managing Victorian-era song lyrics collection.
/// Provides advanced search, filtering, and management capabilities.
/// </summary>
[ApiController]
[Route("api/lyrics-management")]
public class LyricsManagementController : ApiControllerBase
{
    private readonly ILyricsManagementService _lyricsManagement;
    private readonly IInputValidator _inputValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LyricsManagementController"/> class.
    /// </summary>
    /// <param name="lyricsManagement">Service for managing lyrics collection.</param>
    /// <param name="inputValidator">Service for validating and sanitizing user input.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    public LyricsManagementController(
        ILyricsManagementService lyricsManagement,
        IInputValidator inputValidator,
        ILogger<LyricsManagementController> logger) : base(logger)
    {
        _lyricsManagement = lyricsManagement;
        _inputValidator = inputValidator;
    }

    /// <summary>
    /// Gets the complete lyrics collection including all songs, artists, and albums.
    /// </summary>
    /// <returns>The lyrics collection with cached data.</returns>
    /// <response code="200">Returns the lyrics collection.</response>
    [HttpGet("collections")]
    [ProducesResponseType(typeof(LyricsCollection), StatusCodes.Status200OK)]
    public Task<ActionResult<LyricsCollection>> GetLyricsCollection()
    {
        return ExecuteWithErrorHandlingAsync(
            () => _lyricsManagement.LoadLyricsCollectionAsync(),
            "load lyrics collection",
            "Failed to load lyrics collection");
    }

    /// <summary>
    /// Searches for songs matching the query string.
    /// </summary>
    /// <param name="query">Search query to match against song titles, artists, or lyrics.</param>
    /// <param name="maxResults">Maximum number of results to return (1-100, default: 10).</param>
    /// <returns>A list of matching songs.</returns>
    /// <response code="200">Returns the matching songs.</response>
    /// <response code="400">If the query or maxResults parameters are invalid.</response>
    [HttpGet("songs")]
    [ProducesResponseType(typeof(List<Song>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<Song>>> SearchLyrics(
        [FromQuery] string? query = null,
        [FromQuery] int maxResults = 10)
    {
        // Phase 9: Security - Validate search query
        var queryValidation = _inputValidator.ValidateSearchQuery(query);
        if (!queryValidation.IsValid)
        {
            return BadRequest(new { errors = queryValidation.Errors });
        }

        // Phase 9: Security - Validate maxResults parameter
        var maxResultsValidation = _inputValidator.ValidateNumericParameter(
            maxResults, min: 1, max: 100, parameterName: "maxResults");
        if (!maxResultsValidation.IsValid)
        {
            return BadRequest(new { errors = maxResultsValidation.Errors });
        }

        return await ExecuteWithErrorHandlingAsync(
            () => _lyricsManagement.SearchLyricsAsync(queryValidation.SanitizedValue ?? "", maxResultsValidation.Value),
            $"search lyrics for query: {queryValidation.SanitizedValue}",
            "Failed to search lyrics");
    }

    /// <summary>
    /// Gets a specific song by its unique identifier.
    /// </summary>
    /// <param name="id">The song identifier.</param>
    /// <returns>The requested song with full details.</returns>
    /// <response code="200">Returns the song.</response>
    /// <response code="400">If the song ID is invalid.</response>
    /// <response code="404">If the song is not found.</response>
    [HttpGet("songs/{id}")]
    [ProducesResponseType(typeof(Song), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Song>> GetSong(string id)
    {
        // Phase 9: Security - Validate resource ID
        var idValidation = _inputValidator.ValidateResourceId(id);
        if (!idValidation.IsValid)
        {
            return BadRequest(new { errors = idValidation.Errors });
        }

        return await ExecuteWithErrorHandlingAsync(
            () => _lyricsManagement.GetSongByIdAsync(idValidation.SanitizedValue!),
            $"get song by ID: {idValidation.SanitizedValue}",
            "Failed to get song",
            $"Song with ID '{idValidation.SanitizedValue}' not found");
    }

    /// <summary>
    /// Gets the list of all unique artists in the collection.
    /// </summary>
    /// <returns>A list of artist names.</returns>
    /// <response code="200">Returns the list of artists.</response>
    [HttpGet("artists")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public Task<ActionResult<List<string>>> GetArtists()
    {
        return ExecuteWithErrorHandlingAsync(
            () => _lyricsManagement.GetAvailableArtistsAsync(),
            "get available artists",
            "Failed to get artists");
    }

    /// <summary>
    /// Gets the list of all unique albums in the collection.
    /// </summary>
    /// <returns>A list of album names.</returns>
    /// <response code="200">Returns the list of albums.</response>
    [HttpGet("albums")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public Task<ActionResult<List<string>>> GetAlbums()
    {
        return ExecuteWithErrorHandlingAsync(
            () => _lyricsManagement.GetAvailableAlbumsAsync(),
            "get available albums",
            "Failed to get albums");
    }

    /// <summary>
    /// Regenerates the lyrics collection from source files and clears the cache.
    /// </summary>
    /// <returns>Success message when regeneration completes.</returns>
    /// <response code="200">Returns success message.</response>
    [HttpPost("collections/regenerate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<ActionResult> RegenerateCollection()
    {
        return ExecuteWithErrorHandlingAsync(
            () => _lyricsManagement.RegenerateLyricsCollectionAsync(),
            "regenerate lyrics collection",
            "Failed to regenerate lyrics collection",
            "Lyrics collection regenerated successfully");
    }

    /// <summary>
    /// Gets statistical information about the lyrics collection.
    /// </summary>
    /// <returns>Collection statistics including counts, top artists, and popular tags.</returns>
    /// <response code="200">Returns the collection statistics.</response>
    /// <response code="500">If an error occurs retrieving stats.</response>
    [HttpGet("collections/stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
