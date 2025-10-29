using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;

namespace Po.VicTranslate.Api.HealthChecks;

/// <summary>
/// Health check for Azure OpenAI service configuration
/// </summary>
public class AzureOpenAIHealthCheck : IHealthCheck
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<AzureOpenAIHealthCheck> _logger;

    public AzureOpenAIHealthCheck(IOptions<ApiSettings> apiSettings, ILogger<AzureOpenAIHealthCheck> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking Azure OpenAI health");

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
                return Task.FromResult(HealthCheckResult.Healthy("Azure OpenAI configuration is valid"));
            }
            else
            {
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

                var message = $"Configuration values missing or placeholders: {string.Join(", ", missing)}";
                _logger.LogWarning("Azure OpenAI health check failed: {Message}", message);
                return Task.FromResult(HealthCheckResult.Unhealthy(message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI health check failed with exception");
            return Task.FromResult(HealthCheckResult.Unhealthy("Error validating Azure OpenAI configuration", ex));
        }
    }
}
