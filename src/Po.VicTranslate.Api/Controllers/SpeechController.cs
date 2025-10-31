using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;

namespace Po.VicTranslate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeechController : ControllerBase
{
    private readonly IAudioSynthesisService _audioSynthesisService;
    private readonly ILogger<SpeechController> _logger;
    private readonly ApiSettings _apiSettings;

    public SpeechController(IAudioSynthesisService audioSynthesisService, ILogger<SpeechController> logger, IOptions<ApiSettings> apiSettings)
    {
        _audioSynthesisService = audioSynthesisService;
        _logger = logger;
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    [HttpGet("test-config")]
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

    [HttpPost("synthesize")]
    public async Task<IActionResult> SynthesizeSpeech([FromBody] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("Text cannot be empty.");
        }

        try
        {
            _logger.LogInformation("Received request to synthesize speech for text: '{Text}'", text);
            byte[] audioBytes = await _audioSynthesisService.SynthesizeSpeechAsync(text);
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
