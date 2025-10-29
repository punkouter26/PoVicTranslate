using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.CognitiveServices.Speech;
using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Service for validating application configuration settings
/// </summary>
public class ConfigurationValidator : IConfigurationValidator
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<ConfigurationValidator> _logger;

    public ConfigurationValidator(IOptions<ApiSettings> apiSettings, ILogger<ConfigurationValidator> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public DiagnosticResult ValidateAzureOpenAI()
    {
        var result = new DiagnosticResult { CheckName = "Azure OpenAI Configuration" };
        _logger.LogInformation("Validating Azure OpenAI configuration...");

        try
        {
            bool keyOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIApiKey) &&
                        _apiSettings.AzureOpenAIApiKey != "YOUR_AZURE_OPENAI_API_KEY";
            bool endpointOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIEndpoint) &&
                             _apiSettings.AzureOpenAIEndpoint != "YOUR_AZURE_OPENAI_ENDPOINT";
            bool deploymentOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIDeploymentName) &&
                               _apiSettings.AzureOpenAIDeploymentName != "YOUR_AZURE_OPENAI_DEPLOYMENT_NAME";

            if (keyOk && endpointOk && deploymentOk)
            {
                result.Success = true;
                result.Message = "Configuration values (Key, Endpoint, Deployment) are present.";
                _logger.LogInformation("Azure OpenAI configuration validation successful.");
            }
            else
            {
                result.Success = false;
                var missing = new List<string>();
                if (!keyOk)
                {
                    missing.Add("ApiKey");
                }

                if (!endpointOk)
                {
                    missing.Add("Endpoint");
                }

                if (!deploymentOk)
                {
                    missing.Add("DeploymentName");
                }

                result.Message = $"Configuration values missing or placeholders: {string.Join(", ", missing)}.";
                _logger.LogWarning("Azure OpenAI configuration validation failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "An error occurred while validating Azure OpenAI configuration.";
            result.Error = ex;
            _logger.LogError(ex, result.Message);
        }

        return result;
    }

    public async Task<DiagnosticResult> ValidateAzureSpeechAsync()
    {
        var result = new DiagnosticResult { CheckName = "Azure Speech Service Connection" };
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
            var speechConfig = SpeechConfig.FromSubscription(_apiSettings.AzureSpeechSubscriptionKey, _apiSettings.AzureSpeechRegion);
            synthesizer = new SpeechSynthesizer(speechConfig, null);

            _logger.LogInformation("Attempting to retrieve voices from Azure Speech region {Region}...", _apiSettings.AzureSpeechRegion);
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

    public DiagnosticResult ValidateInternetConnectivity()
    {
        var result = new DiagnosticResult { CheckName = "Internet Connectivity" };
        _logger.LogInformation("Validating internet connectivity...");

        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = ping.Send("8.8.8.8", 2000);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                result.Success = true;
                result.Message = "Internet connection appears to be available.";
                _logger.LogInformation(result.Message);
            }
            else
            {
                result.Success = false;
                result.Message = $"Ping failed. Status: {reply.Status}.";
                _logger.LogWarning("Internet connectivity validation failed: {Message}", result.Message);
            }
        }
        catch (System.Net.NetworkInformation.PingException pex)
        {
            result.Success = false;
            result.Message = $"Ping failed with exception: {pex.Message}";
            result.Error = pex;
            _logger.LogError(pex, "Internet connectivity validation failed with PingException.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"An unexpected error occurred during internet check: {ex.Message}";
            result.Error = ex;
            _logger.LogError(ex, "Internet connectivity validation failed with unexpected exception.");
        }

        return result;
    }
}
