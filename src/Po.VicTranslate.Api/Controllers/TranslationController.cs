using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Services.Validation;
using System.Diagnostics;

namespace Po.VicTranslate.Api.Controllers;

/// <summary>
/// Controller for translating modern English text to Victorian-era English.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly ITranslationService _translationService;
    private readonly IAudioSynthesisService _audioSynthesisService;
    private readonly ICustomTelemetryService _telemetryService;
    private readonly IInputValidator _inputValidator;
    private readonly ILogger<TranslationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationController"/> class.
    /// </summary>
    /// <param name="translationService">Service for performing text translation.</param>
    /// <param name="audioSynthesisService">Service for text-to-speech synthesis.</param>
    /// <param name="telemetryService">Service for tracking custom telemetry.</param>
    /// <param name="inputValidator">Service for validating and sanitizing user input.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    public TranslationController(
        ITranslationService translationService,
        IAudioSynthesisService audioSynthesisService,
        ICustomTelemetryService telemetryService,
        IInputValidator inputValidator,
        ILogger<TranslationController> logger)
    {
        _translationService = translationService;
        _audioSynthesisService = audioSynthesisService;
        _telemetryService = telemetryService;
        _inputValidator = inputValidator;
        _logger = logger;
    }

    /// <summary>
    /// Translates modern English text to Victorian-era English and generates audio.
    /// </summary>
    /// <param name="request">The translation request containing the text to translate.</param>
    /// <returns>A <see cref="TranslationResponse"/> containing the translated text and audio.</returns>
    /// <response code="200">Returns the successfully translated text and audio.</response>
    /// <response code="400">If the request is invalid or validation fails.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TranslationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Phase 9: Security - Validate and sanitize text input
        var textValidation = _inputValidator.ValidateTextContent(request.Text, maxLength: 5000);
        if (!textValidation.IsValid)
        {
            _logger.LogWarning("Translation request rejected due to validation errors: {Errors}",
                string.Join(", ", textValidation.Errors));
            return BadRequest(new { errors = textValidation.Errors });
        }

        var sanitizedText = textValidation.SanitizedValue!;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing translation request: TextLength={Length}", sanitizedText.Length);

            var translatedText = await _translationService.TranslateToVictorianEnglishAsync(sanitizedText);

            // Automatically synthesize speech for the translated text
            byte[]? audioData = null;
            try
            {
                _logger.LogInformation("Synthesizing speech for translated text");
                audioData = await _audioSynthesisService.SynthesizeSpeechAsync(translatedText);
                _logger.LogInformation("Speech synthesis successful, audio size: {Size} bytes", audioData.Length);
            }
            catch (Exception audioEx)
            {
                // Don't fail the entire request if TTS fails
                _logger.LogWarning(audioEx, "Speech synthesis failed, but translation succeeded");
            }

            stopwatch.Stop();

            // Phase 4: Track custom telemetry
            _telemetryService.TrackTranslationRequest(
                inputLanguage: "Modern English",
                textLength: sanitizedText.Length,
                success: true,
                durationMs: stopwatch.ElapsedMilliseconds);

            _telemetryService.TrackUserActivity("Translation", userId: null);

            return Ok(new TranslationResponse
            {
                TranslatedText = translatedText,
                AudioData = audioData
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Phase 4: Track failed translation
            _telemetryService.TrackTranslationRequest(
                inputLanguage: "Modern English",
                textLength: sanitizedText.Length,
                success: false,
                durationMs: stopwatch.ElapsedMilliseconds);

            _logger.LogError(ex, "Translation failed for text length {Length}", sanitizedText.Length);
            throw;
        }
    }
}
