using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;

namespace Po.VicTranslate.Api.Controllers;

/// <summary>
/// Controller for text-to-speech synthesis using Azure Cognitive Services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SpeechController : ControllerBase
{
    private readonly IAudioSynthesisService _audioSynthesisService;
    private readonly IInputValidator _inputValidator;
    private readonly ILogger<SpeechController> _logger;
    private readonly ApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeechController"/> class.
    /// </summary>
    /// <param name="audioSynthesisService">Service for synthesizing audio from text.</param>
    /// <param name="inputValidator">Service for validating and sanitizing user input.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <param name="apiSettings">API configuration settings.</param>
    public SpeechController(
        IAudioSynthesisService audioSynthesisService,
        IInputValidator inputValidator,
        ILogger<SpeechController> logger,
        IOptions<ApiSettings> apiSettings)
    {
        _audioSynthesisService = audioSynthesisService;
        _inputValidator = inputValidator;
        _logger = logger;
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <summary>
    /// Tests the Azure Speech Service configuration.
    /// </summary>
    /// <returns>Configuration status including whether key and region are set.</returns>
    /// <response code="200">Returns the configuration status.</response>
    [HttpGet("test-config")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult TestConfiguration()
    {
        var hasKey = !string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechSubscriptionKey);
        var hasRegion = !string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechRegion);
        
        return Ok(new
        {
            HasSubscriptionKey = hasKey,
            KeyPrefix = hasKey ? _apiSettings.AzureSpeechSubscriptionKey.Substring(0, Math.Min(8, _apiSettings.AzureSpeechSubscriptionKey.Length)) + "..." : "MISSING",
            Region = _apiSettings.AzureSpeechRegion ?? "MISSING",
            ConfigurationValid = hasKey && hasRegion
        });
    }

    /// <summary>
    /// Synthesizes speech audio from text using Azure Cognitive Services.
    /// </summary>
    /// <param name="text">The text to convert to speech (max 3000 characters).</param>
    /// <returns>The synthesized audio as MP3 bytes.</returns>
    /// <response code="200">Returns the audio file as application/octet-stream.</response>
    /// <response code="400">If the text is invalid or validation fails.</response>
    /// <response code="500">If speech synthesis fails.</response>
    [HttpPost]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SynthesizeSpeech([FromBody] string text)
    {
        // Phase 9: Security - Validate and sanitize text input
        var textValidation = _inputValidator.ValidateTextContent(text, maxLength: 3000);
        if (!textValidation.IsValid)
        {
            _logger.LogWarning("Speech synthesis request rejected due to validation errors: {Errors}",
                string.Join(", ", textValidation.Errors));
            return BadRequest(new { errors = textValidation.Errors });
        }

        var sanitizedText = textValidation.SanitizedValue!;

        try
        {
            _logger.LogInformation("Received request to synthesize speech for text: '{Text}'", sanitizedText);
            byte[] audioBytes = await _audioSynthesisService.SynthesizeSpeechAsync(sanitizedText);
            _logger.LogInformation("Successfully synthesized speech. Audio size: {Size} bytes", audioBytes.Length);

            // Try to save audio for debugging (optional, may fail in Azure)
            try
            {
                var audioOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "audio_output");
                Directory.CreateDirectory(audioOutputPath);
                var fileName = $"speech_{Guid.NewGuid()}.mp3";
                var filePath = Path.Combine(audioOutputPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, audioBytes);
                _logger.LogInformation("Saved synthesized audio to: {FilePath}", filePath);
            }
            catch (Exception saveEx)
            {
                _logger.LogWarning(saveEx, "Could not save audio file (this is optional and can be ignored)");
            }

            return File(audioBytes, "audio/mpeg"); // MP3 format
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Speech service not configured.");
            return StatusCode(500, $"Speech service is not configured correctly: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing speech. Type: {ExceptionType}, Message: {Message}", ex.GetType().Name, ex.Message);
            return StatusCode(500, $"An error occurred during speech synthesis: {ex.Message}");
        }
    }
}
