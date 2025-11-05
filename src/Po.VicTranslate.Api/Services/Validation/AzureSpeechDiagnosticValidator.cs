using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services.Validation;

/// <summary>
/// Validates Azure Speech Service configuration and connectivity
/// </summary>
public class AzureSpeechDiagnosticValidator : IDiagnosticValidator
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<AzureSpeechDiagnosticValidator> _logger;

    public string CheckName => "Azure Speech Service Connection";

    public AzureSpeechDiagnosticValidator(
        IOptions<ApiSettings> apiSettings,
        ILogger<AzureSpeechDiagnosticValidator> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public async Task<DiagnosticResult> ValidateAsync()
    {
        var result = new DiagnosticResult { CheckName = CheckName };
        _logger.LogInformation("Validating Azure Speech Service connection...");

        if (string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechSubscriptionKey) ||
            _apiSettings.AzureSpeechSubscriptionKey == "YOUR_AZURE_SPEECH_KEY_PLACEHOLDER" ||
            string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechRegion))
        {
            result.Success = false;
            result.Message = "Azure Speech SubscriptionKey or Region is missing or not configured in ApiSettings.";
            _logger.LogWarning("Azure Speech validation failed: {Message}", result.Message);
            return result;
        }

        SpeechSynthesizer? synthesizer = null;
        try
        {
            var speechConfig = SpeechConfig.FromSubscription(
                _apiSettings.AzureSpeechSubscriptionKey,
                _apiSettings.AzureSpeechRegion);
            synthesizer = new SpeechSynthesizer(speechConfig, null);

            _logger.LogInformation("Attempting to retrieve voices from Azure Speech region {Region}...",
                _apiSettings.AzureSpeechRegion);
            using var voiceResult = await synthesizer.GetVoicesAsync();

            if (voiceResult.Reason == ResultReason.VoicesListRetrieved && voiceResult.Voices.Any())
            {
                result.Success = true;
                result.Message = $"Successfully connected and retrieved {voiceResult.Voices.Count} voices from region '{_apiSettings.AzureSpeechRegion}'.";
                _logger.LogInformation("Azure Speech validation successful.");
            }
            else if (voiceResult.Reason == ResultReason.Canceled)
            {
                result.Success = false;
                result.Message = $"Failed to retrieve voices. Reason: Canceled. Details: {voiceResult.ErrorDetails}";
                result.Error = new Exception(result.Message);
                _logger.LogError("Azure Speech validation failed: {Message}", result.Message);
            }
            else
            {
                result.Success = false;
                result.Message = $"Voice list retrieval returned unexpected status: {voiceResult.Reason}";
                result.Error = new Exception(result.Message);
                _logger.LogWarning("Azure Speech validation failed with unexpected status: {Reason}", voiceResult.Reason);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"An error occurred while connecting to Azure Speech Service: {ex.Message}";
            result.Error = ex;
            _logger.LogError(ex, "Azure Speech validation failed with exception.");
        }
        finally
        {
            synthesizer?.Dispose();
        }

        return result;
    }
}
