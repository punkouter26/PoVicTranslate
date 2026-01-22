using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PoVicTranslate.Web.Configuration;

namespace PoVicTranslate.Web.HealthChecks;

/// <summary>
/// Health check for Azure OpenAI service.
/// </summary>
public sealed class AzureOpenAIHealthCheck : IHealthCheck
{
    private readonly ApiSettings _settings;
    private readonly ILogger<AzureOpenAIHealthCheck> _logger;

    public AzureOpenAIHealthCheck(IOptions<ApiSettings> settings, ILogger<AzureOpenAIHealthCheck> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isConfigured = !string.IsNullOrWhiteSpace(_settings.AzureOpenAIApiKey) &&
                           !string.IsNullOrWhiteSpace(_settings.AzureOpenAIEndpoint) &&
                           !string.IsNullOrWhiteSpace(_settings.AzureOpenAIDeploymentName);

        if (!isConfigured)
        {
            _logger.LogWarning("Azure OpenAI is not configured");
            return Task.FromResult(HealthCheckResult.Degraded("Azure OpenAI is not configured"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Azure OpenAI is configured"));
    }
}
