using Microsoft.AspNetCore.Mvc;
using VictorianTranslator.Services;
using VictorianTranslator.Server.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VictorianTranslator.Server.Controllers
{
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
                return Ok(song.Content);
            }

            // Fallback to original service for backward compatibility
            var lyrics = await _lyricsService.GetLyricsAsync(songFileName);
            if (lyrics == null)
            {
                return NotFound();
            }
            return Ok(lyrics);
        }
    }
}
