using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Microsoft.CognitiveServices.Speech;
using System.Text;

namespace Po.VicTranslate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DiagnosticController : ControllerBase
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(IOptions<ApiSettings> apiSettings, ILogger<DiagnosticController> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    [HttpGet("speech-test")]
    public async Task<IActionResult> TestSpeechService()
    {
        var result = new StringBuilder();
        result.AppendLine("=== Azure Speech Service Diagnostic ===\n");

        try
        {
            // Check configuration
            var hasKey = !string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechSubscriptionKey);
            var hasRegion = !string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechRegion);
            
            result.AppendLine($"Configuration:");
            result.AppendLine($"  Has Key: {hasKey}");
            result.AppendLine($"  Key Length: {_apiSettings.AzureSpeechSubscriptionKey?.Length ?? 0}");
            result.AppendLine($"  Region: {_apiSettings.AzureSpeechRegion}");
            result.AppendLine();

            if (!hasKey || !hasRegion)
            {
                result.AppendLine("ERROR: Missing configuration");
                return Ok(result.ToString());
            }

            // Try to create speech config
            result.AppendLine("Creating SpeechConfig...");
            var speechConfig = SpeechConfig.FromSubscription(_apiSettings.AzureSpeechSubscriptionKey, _apiSettings.AzureSpeechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-GB-RyanNeural";
            result.AppendLine($"  Voice: {speechConfig.SpeechSynthesisVoiceName}");
            result.AppendLine($"  Region: {speechConfig.Region}");
            result.AppendLine("  SUCCESS");
            result.AppendLine();

            // Try to create synthesizer
            result.AppendLine("Creating SpeechSynthesizer...");
            using var synthesizer = new SpeechSynthesizer(speechConfig, null);
            result.AppendLine("  SUCCESS");
            result.AppendLine();

            // Try synthesis
            result.AppendLine("Testing synthesis...");
            var testText = "Hello world";
            var synthResult = await synthesizer.SpeakTextAsync(testText);
            
            result.AppendLine($"  Result Reason: {synthResult.Reason}");
            result.AppendLine($"  Audio Data Length: {synthResult.AudioData?.Length ?? 0}");

            if (synthResult.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                result.AppendLine("  SUCCESS - Synthesis completed!");
            }
            else if (synthResult.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(synthResult);
                result.AppendLine($"  FAILED - Canceled");
                result.AppendLine($"  Cancellation Reason: {cancellation.Reason}");
                result.AppendLine($"  Error Code: {cancellation.ErrorCode}");
                result.AppendLine($"  Error Details: {cancellation.ErrorDetails}");
            }
            else
            {
                result.AppendLine($"  FAILED - Unexpected reason: {synthResult.Reason}");
            }

        }
        catch (Exception ex)
        {
            result.AppendLine($"\nEXCEPTION: {ex.Message}");
            result.AppendLine($"Type: {ex.GetType().Name}");
            result.AppendLine($"Stack Trace:\n{ex.StackTrace}");
        }

        return Ok(result.ToString());
    }
}
