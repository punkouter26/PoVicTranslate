using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Microsoft.Extensions.Logging;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Azure Speech Services implementation for audio synthesis
/// </summary>
public class AudioSynthesisService : IAudioSynthesisService
{
    private readonly SpeechConfig _speechConfig;
    private readonly ILogger<AudioSynthesisService> _logger;

    public AudioSynthesisService(IOptions<ApiSettings> apiSettings, ILogger<AudioSynthesisService> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _logger = logger;
        var settings = apiSettings.Value;

        if (string.IsNullOrWhiteSpace(settings.AzureSpeechSubscriptionKey) ||
            string.IsNullOrWhiteSpace(settings.AzureSpeechRegion))
        {
            _logger.LogError("Azure Speech settings (SubscriptionKey, Region) are not configured properly in appsettings.json.");
            throw new InvalidOperationException("Azure Speech settings are not configured.");
        }

        _speechConfig = SpeechConfig.FromSubscription(settings.AzureSpeechSubscriptionKey, settings.AzureSpeechRegion);
        _speechConfig.SpeechSynthesisVoiceName = "en-GB-LibbyNeural"; // A suitable Victorian-sounding voice
        _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3); // Changed to a different MP3 format
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text)
    {
        _logger.LogInformation("Starting speech synthesis for text: '{Text}'", text);

        using (var synthesizer = new SpeechSynthesizer(_speechConfig, null))
        {
            var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation("Speech synthesis completed successfully.");
                using (var audioStream = AudioDataStream.FromResult(result))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[8000]; // Buffer size
                        uint bytesRead;
                        while ((bytesRead = audioStream.ReadData(buffer)) > 0)
                        {
                            memoryStream.Write(buffer, 0, (int)bytesRead);
                        }
                        return memoryStream.ToArray();
                    }
                }
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                _logger.LogError("Speech synthesis cancelled: Reason={Reason}, ErrorDetails={ErrorDetails}",
                    cancellation.Reason, cancellation.ErrorDetails);
                throw new Exception($"Speech synthesis cancelled: {cancellation.Reason}. Details: {cancellation.ErrorDetails}");
            }
            else
            {
                _logger.LogError("Speech synthesis failed: Reason={Reason}", result.Reason);
                throw new Exception($"Speech synthesis failed: {result.Reason}");
            }
        }
    }
}
