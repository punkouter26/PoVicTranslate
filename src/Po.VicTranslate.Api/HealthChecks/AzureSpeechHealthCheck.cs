using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.CognitiveServices.Speech;
using Po.VicTranslate.Api.Configuration;

namespace Po.VicTranslate.Api.HealthChecks;

/// <summary>
/// Health check for Azure Speech service connectivity
/// </summary>
public class AzureSpeechHealthCheck : IHealthCheck
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<AzureSpeechHealthCheck> _logger;

    public AzureSpeechHealthCheck(IOptions<ApiSettings> apiSettings, ILogger<AzureSpeechHealthCheck> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking Azure Speech service health");

        if (string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechSubscriptionKey) ||
            _apiSettings.AzureSpeechSubscriptionKey == "YOUR_AZURE_SPEECH_KEY_PLACEHOLDER" ||
            string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechRegion))
        {
            var message = "Azure Speech SubscriptionKey or Region is missing or not configured";
            _logger.LogWarning("Azure Speech health check failed: {Message}", message);
            return HealthCheckResult.Unhealthy(message);
        }

        SpeechSynthesizer? synthesizer = null;
        try
        {
            var speechConfig = SpeechConfig.FromSubscription(_apiSettings.AzureSpeechSubscriptionKey, _apiSettings.AzureSpeechRegion);
            synthesizer = new SpeechSynthesizer(speechConfig, null);

            _logger.LogInformation("Attempting to retrieve voices from Azure Speech region {Region}", _apiSettings.AzureSpeechRegion);
            using var voiceResult = await synthesizer.GetVoicesAsync();

            if (voiceResult.Reason == ResultReason.VoicesListRetrieved && voiceResult.Voices.Any())
            {
                var message = $"Successfully connected and retrieved {voiceResult.Voices.Count} voices from region '{_apiSettings.AzureSpeechRegion}'";
                _logger.LogInformation("Azure Speech health check successful");
                return HealthCheckResult.Healthy(message);
            }
            else if (voiceResult.Reason == ResultReason.Canceled)
            {
                var message = $"Failed to retrieve voices. Reason: Canceled. Details: {voiceResult.ErrorDetails}";
                _logger.LogError("Azure Speech health check failed: {Message}", message);
                return HealthCheckResult.Unhealthy(message);
            }
            else
            {
                var message = $"Voice list retrieval returned unexpected status: {voiceResult.Reason}";
                _logger.LogWarning("Azure Speech health check failed with unexpected status: {Reason}", voiceResult.Reason);
                return HealthCheckResult.Degraded(message);
            }
        }
        catch (TypeInitializationException ex)
        {
            // This typically means native Speech SDK libraries are not available
            _logger.LogWarning(ex, "Azure Speech SDK native libraries not available in this environment");
            return HealthCheckResult.Degraded("Azure Speech SDK native libraries not available. Speech synthesis is disabled.", ex);
        }
        catch (DllNotFoundException ex)
        {
            _logger.LogWarning(ex, "Azure Speech SDK DLL not found");
            return HealthCheckResult.Degraded("Azure Speech SDK native libraries not found. Speech synthesis is disabled.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Speech health check failed with exception");
            return HealthCheckResult.Unhealthy($"Error connecting to Azure Speech Service: {ex.Message}", ex);
        }
        finally
        {
            synthesizer?.Dispose();
        }
    }
}
