using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Lyrics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Po.VicTranslate.Api.Controllers;

/// <summary>
/// Controller for retrieving Victorian-era song lyrics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LyricsController : ControllerBase
{
    private readonly ILyricsService _lyricsService;
    private readonly ILyricsUtilityService _lyricsUtility;

    /// <summary>
    /// Initializes a new instance of the <see cref="LyricsController"/> class.
    /// </summary>
    /// <param name="lyricsService">Service for retrieving song lyrics.</param>
    /// <param name="lyricsUtility">Utility service for lyrics operations.</param>
    public LyricsController(
        ILyricsService lyricsService,
        ILyricsUtilityService lyricsUtility)
    {
        _lyricsService = lyricsService;
        _lyricsUtility = lyricsUtility;
    }

    /// <summary>
    /// Gets the list of all available song titles.
    /// </summary>
    /// <returns>A list of song identifiers.</returns>
    /// <response code="200">Returns the list of available songs.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetAvailableSongs()
    {
        var songs = await _lyricsService.GetAvailableSongsAsync();
        return Ok(songs);
    }

    /// <summary>
    /// Gets the lyrics for a specific song, limited to 200 words.
    /// </summary>
    /// <param name="songFileName">The song identifier/filename.</param>
    /// <returns>The song lyrics limited to 200 words.</returns>
    /// <response code="200">Returns the song lyrics.</response>
    /// <response code="404">If the song is not found.</response>
    [HttpGet("{songFileName}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetLyrics(string songFileName)
    {
        var lyrics = await _lyricsService.GetLyricsAsync(songFileName);
        if (lyrics == null)
        {
            return NotFound();
        }
        
        // Limit to 200 words
        var limitedLyrics = _lyricsUtility.LimitWords(lyrics, 200);
        return Ok(limitedLyrics);
    }
}
