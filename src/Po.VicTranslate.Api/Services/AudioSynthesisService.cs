using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Services.Validation;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Azure Speech Services implementation for audio synthesis.
/// Uses lazy initialization to avoid native DLL loading issues in environments
/// where the Speech SDK native libraries may not be available.
/// </summary>
public class AudioSynthesisService : IAudioSynthesisService
{
    private readonly ApiSettings _settings;
    private readonly ISpeechConfigValidator _configValidator;
    private readonly ILogger<AudioSynthesisService> _logger;
    private SpeechConfig? _speechConfig;
    private bool _initializationAttempted;
    private Exception? _initializationException;

    public AudioSynthesisService(
        IOptions<ApiSettings> apiSettings,
        ISpeechConfigValidator configValidator,
        ILogger<AudioSynthesisService> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        ArgumentNullException.ThrowIfNull(configValidator);
        _settings = apiSettings.Value;
        _configValidator = configValidator;
        _logger = logger;

        // Defer initialization to first use to avoid native DLL loading at startup
        _logger.LogInformation("AudioSynthesisService created with lazy initialization");
    }

    private SpeechConfig GetOrCreateSpeechConfig()
    {
        if (_speechConfig != null)
            return _speechConfig;

        if (_initializationAttempted && _initializationException != null)
            throw _initializationException;

        _initializationAttempted = true;

        // Delegate validation to the validator (SRP)
        if (!_configValidator.IsValid(_settings))
        {
            var error = _configValidator.GetValidationError(_settings);
            _logger.LogError("Azure Speech settings validation failed: {Error}", error);
            _initializationException = new InvalidOperationException($"Azure Speech settings are not configured: {error}");
            throw _initializationException;
        }

        try
        {
            _logger.LogInformation("Initializing Azure Speech Service with Region: {Region}", _settings.AzureSpeechRegion);
            _speechConfig = SpeechConfig.FromSubscription(_settings.AzureSpeechSubscriptionKey, _settings.AzureSpeechRegion);

            // Try using a more standard British English voice that's widely available
            _speechConfig.SpeechSynthesisVoiceName = "en-GB-RyanNeural";
            _logger.LogInformation("Using voice: {Voice}", _speechConfig.SpeechSynthesisVoiceName);

            // Use standard MP3 format for better compatibility
            _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

            return _speechConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure Speech SDK. This may be due to missing native libraries.");
            _initializationException = new InvalidOperationException(
                "Azure Speech SDK initialization failed. The native libraries may not be available in this environment.", ex);
            throw _initializationException;
        }
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text)
    {
        var speechConfig = GetOrCreateSpeechConfig();

        _logger.LogInformation("Starting speech synthesis for text: '{Text}'", text);
        _logger.LogInformation("Using voice: {Voice}, Region: {Region}",
            speechConfig.SpeechSynthesisVoiceName,
            speechConfig.Region);

        using (var synthesizer = new SpeechSynthesizer(speechConfig, null))
        {
            // Use SSML for better reliability and control
            var ssml = $@"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
                <voice name='{speechConfig.SpeechSynthesisVoiceName}'>
                    {System.Security.SecurityElement.Escape(text)}
                </voice>
            </speak>";

            _logger.LogInformation("Using SSML for synthesis");
            var result = await synthesizer.SpeakSsmlAsync(ssml);

            _logger.LogInformation("Speech synthesis result reason: {Reason}", result.Reason);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation("Speech synthesis completed successfully. Audio data length: {Length} bytes", result.AudioData?.Length ?? 0);

                // Directly return the audio data from the result
                if (result.AudioData != null && result.AudioData.Length > 0)
                {
                    return result.AudioData;
                }

                _logger.LogWarning("Audio data is null or empty, falling back to stream reading");
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
                _logger.LogError("Speech synthesis cancelled: Reason={Reason}, ErrorCode={ErrorCode}, ErrorDetails={ErrorDetails}",
                    cancellation.Reason, cancellation.ErrorCode, cancellation.ErrorDetails);

                // Provide more specific error messages
                var errorMessage = cancellation.ErrorCode switch
                {
                    CancellationErrorCode.AuthenticationFailure => "Authentication failed. Please verify your Azure Speech Service subscription key.",
                    CancellationErrorCode.BadRequest => "Bad request. The voice or configuration may not be supported in this region.",
                    CancellationErrorCode.TooManyRequests => "Too many requests. Please try again later.",
                    CancellationErrorCode.Forbidden => "Access forbidden. Please check your subscription permissions.",
                    CancellationErrorCode.ConnectionFailure => "Connection failed. Please check your network connection.",
                    CancellationErrorCode.ServiceTimeout => "Service timeout. Please try again.",
                    _ => $"Speech synthesis cancelled: {cancellation.Reason}. ErrorCode: {cancellation.ErrorCode}. Details: {cancellation.ErrorDetails}"
                };

                throw new Exception(errorMessage);
            }
            else
            {
                _logger.LogError("Speech synthesis failed: Reason={Reason}", result.Reason);
                throw new Exception($"Speech synthesis failed: {result.Reason}");
            }
        }
    }
}
