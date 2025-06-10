using Microsoft.AspNetCore.Mvc;
using VictorianTranslator.Services;
using VictorianTranslator.Models;

namespace VictorianTranslator.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;

        public TranslationController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return BadRequest("Text cannot be empty.");
            }

            var translatedText = await _translationService.TranslateToVictorianEnglishAsync(request.Text);
            return Ok(new TranslationResponse { TranslatedText = translatedText });
        }
    }
}
