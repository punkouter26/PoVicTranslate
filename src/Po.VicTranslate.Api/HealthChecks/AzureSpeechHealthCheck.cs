using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;

namespace Po.VicTranslate.Api.HealthChecks;

/// <summary>
/// Health check for Azure Speech service connectivity using REST API
/// </summary>
public class AzureSpeechHealthCheck : IHealthCheck
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<AzureSpeechHealthCheck> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public AzureSpeechHealthCheck(
        IOptions<ApiSettings> apiSettings, 
        ILogger<AzureSpeechHealthCheck> logger,
        IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking Azure Speech service health via REST API");

        if (string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechSubscriptionKey) ||
            _apiSettings.AzureSpeechSubscriptionKey == "YOUR_AZURE_SPEECH_KEY_PLACEHOLDER" ||
            string.IsNullOrWhiteSpace(_apiSettings.AzureSpeechRegion))
        {
            var message = "Azure Speech SubscriptionKey or Region is missing or not configured";
            _logger.LogWarning("Azure Speech health check failed: {Message}", message);
            return HealthCheckResult.Unhealthy(message);
        }

        try
        {
            // Use the token endpoint to verify credentials are valid
            var tokenEndpoint = $"https://{_apiSettings.AzureSpeechRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
            
            using var httpClient = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _apiSettings.AzureSpeechSubscriptionKey);
            request.Content = new StringContent(string.Empty);

            _logger.LogInformation("Attempting to get token from Azure Speech region {Region}", _apiSettings.AzureSpeechRegion);
            
            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var message = $"Successfully authenticated with Azure Speech Service in region '{_apiSettings.AzureSpeechRegion}'";
                _logger.LogInformation("Azure Speech health check successful");
                return HealthCheckResult.Healthy(message);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = $"Failed to authenticate with Azure Speech Service. Status: {response.StatusCode}. Details: {errorContent}";
                _logger.LogError("Azure Speech health check failed: {Message}", message);
                return HealthCheckResult.Unhealthy(message);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Azure Speech health check failed with HTTP exception");
            return HealthCheckResult.Unhealthy($"Error connecting to Azure Speech Service: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Speech health check failed with exception");
            return HealthCheckResult.Unhealthy($"Error checking Azure Speech Service health: {ex.Message}", ex);
        }
    }
}
