using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Models;
using System.Diagnostics;

namespace Po.VicTranslate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TranslationController : ControllerBase
{
    private readonly ITranslationService _translationService;
    private readonly ICustomTelemetryService _telemetryService;
    private readonly ILogger<TranslationController> _logger;

    public TranslationController(
        ITranslationService translationService,
        ICustomTelemetryService telemetryService,
        ILogger<TranslationController> logger)
    {
        _translationService = translationService;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpPost("translate")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(request.Text))
        {
            _logger.LogWarning("Translation request received with empty text");
            return BadRequest("Text cannot be empty.");
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing translation request: TextLength={Length}", request.Text.Length);

            var translatedText = await _translationService.TranslateToVictorianEnglishAsync(request.Text);

            stopwatch.Stop();

            // Phase 4: Track custom telemetry
            _telemetryService.TrackTranslationRequest(
                inputLanguage: "Modern English",
                textLength: request.Text.Length,
                success: true,
                durationMs: stopwatch.ElapsedMilliseconds);

            _telemetryService.TrackUserActivity("Translation", userId: null);

            return Ok(new TranslationResponse { TranslatedText = translatedText });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Phase 4: Track failed translation
            _telemetryService.TrackTranslationRequest(
                inputLanguage: "Modern English",
                textLength: request.Text.Length,
                success: false,
                durationMs: stopwatch.ElapsedMilliseconds);

            _logger.LogError(ex, "Translation failed for text length {Length}", request.Text.Length);
            throw;
        }
    }
}
