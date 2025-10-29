using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Po.VicTranslate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LyricsController : ControllerBase
{
    private readonly ILyricsService _lyricsService;
    private readonly ILyricsManagementService _lyricsManagement;

    public LyricsController(ILyricsService lyricsService, ILyricsManagementService lyricsManagement)
    {
        _lyricsService = lyricsService;
        _lyricsManagement = lyricsManagement;
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<string>>> GetAvailableSongs()
    {
        // Use new management service for better performance and structure
        var collection = await _lyricsManagement.LoadLyricsCollectionAsync();
        var songTitles = collection.Songs.Select(s => s.Id).ToList();
        return Ok(songTitles);
    }

    [HttpGet("lyrics/{songFileName}")]
    public async Task<ActionResult<string>> GetLyrics(string songFileName)
    {
        // Try new management service first
        var song = await _lyricsManagement.GetSongByIdAsync(songFileName);
        if (song != null)
        {
            // Limit to 200 words
            var limitedContent = LimitWords(song.Content, 200);
            return Ok(limitedContent);
        }

        // Fallback to original service for backward compatibility (already limits to 200 words)
        var lyrics = await _lyricsService.GetLyricsAsync(songFileName);
        if (lyrics == null)
        {
            return NotFound();
        }
        return Ok(lyrics);
    }

    private static string LimitWords(string text, int maxWords)
    {
        var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= maxWords)
        {
            return text;
        }

        return string.Join(" ", words.Take(maxWords)) + "...";
    }
}
