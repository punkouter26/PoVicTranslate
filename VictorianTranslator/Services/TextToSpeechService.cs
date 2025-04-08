using Microsoft.CognitiveServices.Speech;
// using Microsoft.CognitiveServices.Speech.Audio; // Not needed for in-memory synthesis
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop; // Added back for JS interop
using VictorianTranslator.Configuration;

namespace VictorianTranslator.Services;

public class TextToSpeechService : ITextToSpeechService
{
    private readonly SpeechConfig _speechConfig;
    private readonly ILogger<TextToSpeechService> _logger;
    private readonly IJSRuntime _jsRuntime; // Added back
    // Define a suitable default Victorian-esque voice (adjust if needed)
    private const string DefaultVictorianVoice = "en-US-AriaNeural"; // Example, choose appropriate

    public TextToSpeechService(IOptions<ApiSettings> apiSettings, ILogger<TextToSpeechService> logger, IJSRuntime jsRuntime) // Added IJSRuntime
    {
        _logger = logger;
        _jsRuntime = jsRuntime; // Assign IJSRuntime
        var settings = apiSettings.Value;

        if (string.IsNullOrWhiteSpace(settings.AzureSpeechSubscriptionKey) || string.IsNullOrWhiteSpace(settings.AzureSpeechRegion))
        {
            _logger.LogError("Azure Speech SubscriptionKey or Region is not configured. Please check ApiSettings.");
            throw new InvalidOperationException("Azure Speech SubscriptionKey and Region must be configured.");
        }

        try
        {
            _speechConfig = SpeechConfig.FromSubscription(settings.AzureSpeechSubscriptionKey, settings.AzureSpeechRegion);
            // Optional: Set a default voice if desired, or set it per request.
            _speechConfig.SpeechSynthesisVoiceName = DefaultVictorianVoice; 
            // Optional: Set output format if synthesizing to file/stream, not needed for default speaker output.
            // _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);
            _logger.LogInformation("Azure TextToSpeechService initialized for region {Region}.", settings.AzureSpeechRegion);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Failed to initialize Azure SpeechConfig.");
             throw;
        }
    }

    // Removed GetAudioBytesAsync as we will synthesize directly to speaker

    public async Task SpeakTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("SpeakTextAsync called with empty text.");
            return;
        }

        _logger.LogInformation("Attempting to speak text using Azure Speech SDK. Voice: {Voice}. Text length: {Length}", 
            _speechConfig.SpeechSynthesisVoiceName, text.Length);
        _logger.LogTrace("Speaking text: {Text}", text); // Log full text only at Trace level

        try
        {
            // Synthesize audio into memory
            // Passing null for AudioConfig synthesizes to an in-memory buffer accessible via result.AudioData
            using var synthesizer = new SpeechSynthesizer(_speechConfig, null); 
            
            _logger.LogInformation("Synthesizing audio to memory...");
            using var result = await synthesizer.SpeakTextAsync(text); // Use SpeakTextAsync, audio is in result.AudioData

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation("Speech synthesis to memory completed successfully. Audio data length: {Length} bytes.", result.AudioData.Length);
                
                if (result.AudioData.Length == 0)
                {
                    _logger.LogWarning("Synthesized audio data is empty.");
                    throw new Exception("Synthesized audio data is empty.");
                }

                // Convert the audio bytes to a base64 string
                var audioBase64 = Convert.ToBase64String(result.AudioData);
                
                // Play the audio using JavaScript interop
                _logger.LogInformation("Sending audio data to browser via JavaScript interop...");
                await _jsRuntime.InvokeVoidAsync("playAudio", audioBase64);
                _logger.LogInformation("JavaScript interop call completed.");

            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result); // Okay to use here
                _logger.LogError("Speech synthesis canceled. Reason: {Reason}. ErrorDetails: {ErrorDetails}",
                    cancellation.Reason, cancellation.ErrorDetails);

                // Provide more specific feedback if possible
                if (cancellation.Reason == CancellationReason.Error)
                {
                   _logger.LogError("Azure Speech ErrorCode={ErrorCode}", cancellation.ErrorCode);
                   // Rethrow or handle specific errors (e.g., authentication, connection)
                   throw new Exception($"Azure Speech synthesis failed: {cancellation.ErrorDetails} (Code: {cancellation.ErrorCode})");
                }
                 throw new Exception($"Azure Speech synthesis canceled: {cancellation.Reason}");
            }
            else
            {
                 _logger.LogWarning("Speech synthesis resulted in unexpected status: {Reason}", result.Reason);
                 throw new Exception($"Azure Speech synthesis failed with unexpected status: {result.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during Azure speech synthesis.");
            // Rethrow to allow higher-level error handling (e.g., display message in UI)
            throw new Exception("An unexpected error occurred while trying to speak the text.", ex); 
        }
    }
}
