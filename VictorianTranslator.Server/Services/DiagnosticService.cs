using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.CognitiveServices.Speech;
using VictorianTranslator.Configuration;
using VictorianTranslator.Models;
using Azure.AI.OpenAI; // Needed for config check, though client isn't used directly here to avoid rate limits
using Azure; // Needed for AzureKeyCredential

namespace VictorianTranslator.Services;

/// <summary>
/// Service to perform diagnostic checks on application dependencies.
/// </summary>
public class DiagnosticService : IDiagnosticService
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<DiagnosticService> _logger;

    public DiagnosticService(IOptions<ApiSettings> apiSettings, ILogger<DiagnosticService> logger)
    {
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<DiagnosticResult>> RunChecksAsync()
    {
        _logger.LogInformation("Running diagnostic checks...");
        var results = new List<DiagnosticResult>();

        results.Add(CheckAzureOpenAIConfiguration());
        results.Add(await CheckAzureSpeechConnectionAsync());
        // Add more checks here if needed (e.g., Internet connection, Database)
        results.Add(CheckInternetConnection()); // Basic check

        _logger.LogInformation("Diagnostic checks completed.");
        return results;
    }

    /// <summary>
    /// Checks if the Azure OpenAI configuration settings are present.
    /// Does not attempt a connection to avoid rate limiting on S0 tier.
    /// </summary>
    private DiagnosticResult CheckAzureOpenAIConfiguration()
    {
        var result = new DiagnosticResult { CheckName = "Azure OpenAI Configuration" };
        _logger.LogInformation("Checking Azure OpenAI configuration...");

        try
        {
            bool keyOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIApiKey) && _apiSettings.AzureOpenAIApiKey != "YOUR_AZURE_OPENAI_API_KEY";
            bool endpointOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIEndpoint) && _apiSettings.AzureOpenAIEndpoint != "YOUR_AZURE_OPENAI_ENDPOINT";
            bool deploymentOk = !string.IsNullOrWhiteSpace(_apiSettings.AzureOpenAIDeploymentName) && _apiSettings.AzureOpenAIDeploymentName != "YOUR_AZURE_OPENAI_DEPLOYMENT_NAME";

            if (keyOk && endpointOk && deploymentOk)
            {
                result.Success = true;
                result.Message = "Configuration values (Key, Endpoint, Deployment) are present.";
                _logger.LogInformation("Azure OpenAI configuration check successful.");
            }
            else
            {
                result.Success = false;
                var missing = new List<string>();
                if (!keyOk) missing.Add("ApiKey");
                if (!endpointOk) missing.Add("Endpoint");
                if (!deploymentOk) missing.Add("DeploymentName");
                result.Message = $"Configuration values missing or placeholders: {string.Join(", ", missing)}.";
                _logger.LogWarning("Azure OpenAI configuration check failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "An error occurred while checking Azure OpenAI configuration.";
            result.Error = ex;
            _logger.LogError(ex, result.Message);
        }
        return result;
    }

    /// <summary>
    /// Checks the connection to Azure Speech Service by attempting to retrieve voices.
    /// </summary>
    private async Task<DiagnosticResult> CheckAzureSpeechConnectionAsync()
    {
        var result = new DiagnosticResult { CheckName = "Azure Speech Service Connection" };
         _logger.LogInformation("Checking Azure Speech Service connection...");

        if (string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechSubscriptionKey) ||
            _apiSettings.AzureSpeechSubscriptionKey == "YOUR_AZURE_SPEECH_KEY_PLACEHOLDER" || // Example placeholder check
            string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechRegion))
        {
            result.Success = false;
            result.Message = "Azure Speech SubscriptionKey or Region is missing or not configured in ApiSettings.";
             _logger.LogWarning("Azure Speech connection check failed: {Message}", result.Message);
            return result;
        }

        SpeechConfig? speechConfig = null;
        SpeechSynthesizer? synthesizer = null;
        try
        {
            speechConfig = SpeechConfig.FromSubscription(_apiSettings.AzureSpeechSubscriptionKey, _apiSettings.AzureSpeechRegion);
            // Timeout configuration (optional but recommended for diagnostics)
            // speechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "5000"); // 5 seconds
            // speechConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "5000");

            synthesizer = new SpeechSynthesizer(speechConfig, null); // null audio config for voice list retrieval

            _logger.LogInformation("Attempting to retrieve voices from Azure Speech region {Region}...", _apiSettings.AzureSpeechRegion);
            using var voiceResult = await synthesizer.GetVoicesAsync();

            if (voiceResult.Reason == ResultReason.VoicesListRetrieved && voiceResult.Voices.Any())
            {
                result.Success = true;
                result.Message = $"Successfully connected and retrieved {voiceResult.Voices.Count} voices from region '{_apiSettings.AzureSpeechRegion}'.";
                _logger.LogInformation("Azure Speech connection check successful.");
            }
            else if (voiceResult.Reason == ResultReason.Canceled)
            {
                // Cannot use SpeechSynthesisCancellationDetails here. Get error details directly.
                result.Success = false;
                result.Message = $"Failed to retrieve voices. Reason: Canceled. Details: {voiceResult.ErrorDetails}";
                result.Error = new Exception(result.Message); // Wrap error details in an exception
                 _logger.LogError("Azure Speech connection check failed: {Message}", result.Message);
            }
             else
            {
                result.Success = false;
                result.Message = $"Voice list retrieval returned unexpected status: {voiceResult.Reason}";
                result.Error = new Exception(result.Message);
                 _logger.LogWarning("Azure Speech connection check failed with unexpected status: {Reason}", voiceResult.Reason);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"An error occurred while connecting to Azure Speech Service: {ex.Message}";
            result.Error = ex;
            _logger.LogError(ex, "Azure Speech connection check failed with exception.");
        }
        finally
        {
            // Ensure synthesizer is disposed
             synthesizer?.Dispose();
             // SpeechConfig is not IDisposable, remove dispose call
             // speechConfig?.Dispose(); 
        }

        return result;
    }

     /// <summary>
    /// Performs a basic check for internet connectivity by pinging a reliable host.
    /// </summary>
    private DiagnosticResult CheckInternetConnection()
    {
        var result = new DiagnosticResult { CheckName = "Internet Connectivity" };
        _logger.LogInformation("Checking internet connectivity...");
        try
        {
            // Using System.Net.NetworkInformation Ping - requires adding the namespace
            using var ping = new System.Net.NetworkInformation.Ping();
            // Ping a reliable host (e.g., Google's public DNS)
            var reply = ping.Send("8.8.8.8", 2000); // 2 second timeout

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
                 _logger.LogWarning("Internet connectivity check failed: {Message}", result.Message);
            }
        }
        catch (System.Net.NetworkInformation.PingException pex)
        {
             result.Success = false;
             result.Message = $"Ping failed with exception: {pex.Message}";
             result.Error = pex;
             _logger.LogError(pex, "Internet connectivity check failed with PingException.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"An unexpected error occurred during internet check: {ex.Message}";
            result.Error = ex;
            _logger.LogError(ex, "Internet connectivity check failed with unexpected exception.");
        }
        return result;
    }
}
