using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PoVicTranslate.Web.Configuration;

namespace PoVicTranslate.Web.HealthChecks;

/// <summary>
/// Health check for Azure Speech service.
/// </summary>
public sealed class AzureSpeechHealthCheck : IHealthCheck
{
    private readonly ApiSettings _settings;
    private readonly ILogger<AzureSpeechHealthCheck> _logger;

    public AzureSpeechHealthCheck(IOptions<ApiSettings> settings, ILogger<AzureSpeechHealthCheck> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isConfigured = !string.IsNullOrWhiteSpace(_settings.AzureSpeechSubscriptionKey) &&
                           !string.IsNullOrWhiteSpace(_settings.AzureSpeechRegion);

        if (!isConfigured)
        {
            _logger.LogWarning("Azure Speech is not configured");
            return Task.FromResult(HealthCheckResult.Degraded("Azure Speech is not configured"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Azure Speech is configured"));
    }
}
