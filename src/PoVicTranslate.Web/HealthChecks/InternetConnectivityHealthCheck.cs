using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PoVicTranslate.Web.HealthChecks;

/// <summary>
/// Health check for internet connectivity.
/// </summary>
public sealed class InternetConnectivityHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InternetConnectivityHealthCheck> _logger;

    public InternetConnectivityHealthCheck(IHttpClientFactory httpClientFactory, ILogger<InternetConnectivityHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync("https://www.microsoft.com", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Internet connectivity is available");
            }

            _logger.LogWarning("Internet connectivity check returned: {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded($"Internet connectivity returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internet connectivity check failed");
            return HealthCheckResult.Unhealthy("Internet connectivity is not available", ex);
        }
    }
}
