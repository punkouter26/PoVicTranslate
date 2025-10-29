using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Microsoft.Extensions.Logging;

namespace Po.VicTranslate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeechController : ControllerBase
{
    private readonly IAudioSynthesisService _audioSynthesisService;
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(IAudioSynthesisService audioSynthesisService, ILogger<SpeechController> logger)
    {
        _audioSynthesisService = audioSynthesisService;
        _logger = logger;
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
            _logger.LogInformation("Successfully synthesized speech. Returning audio stream.");

            // Save the audio to a file for debugging
            var audioOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "audio_output");
            if (!Directory.Exists(audioOutputPath))
            {
                Directory.CreateDirectory(audioOutputPath);
            }
            var fileName = $"speech_{Guid.NewGuid()}.mp3"; // Changed extension to mp3
            var filePath = Path.Combine(audioOutputPath, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, audioBytes);
            _logger.LogInformation("Saved synthesized audio to: {FilePath}", filePath);

            return File(audioBytes, "audio/mpeg"); // MP3 format
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Speech service not configured.");
            return StatusCode(500, "Speech service is not configured correctly. Please check server logs.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing speech.");
            return StatusCode(500, "An error occurred during speech synthesis. Please check server logs.");
        }
    }
}
