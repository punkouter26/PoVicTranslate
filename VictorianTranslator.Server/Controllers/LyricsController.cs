using Microsoft.AspNetCore.Mvc;
using VictorianTranslator.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VictorianTranslator.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LyricsController : ControllerBase
    {
        private readonly ILyricsService _lyricsService;

        public LyricsController(ILyricsService lyricsService)
        {
            _lyricsService = lyricsService;
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<string>>> GetAvailableSongs()
        {
            var songs = await _lyricsService.GetAvailableSongsAsync();
            return Ok(songs);
        }

        [HttpGet("lyrics/{songFileName}")]
        public async Task<ActionResult<string>> GetLyrics(string songFileName)
        {
            var lyrics = await _lyricsService.GetLyricsAsync(songFileName);
            if (lyrics == null)
            {
                return NotFound();
            }
            return Ok(lyrics);
        }
    }
}
