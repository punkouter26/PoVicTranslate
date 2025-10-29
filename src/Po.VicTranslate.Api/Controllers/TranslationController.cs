using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Controllers;

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
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(request.Text))
        {
            return BadRequest("Text cannot be empty.");
        }

        var translatedText = await _translationService.TranslateToVictorianEnglishAsync(request.Text);
        return Ok(new TranslationResponse { TranslatedText = translatedText });
    }
}
