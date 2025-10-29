using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.NetworkInformation;

namespace Po.VicTranslate.Api.HealthChecks;

/// <summary>
/// Health check for internet connectivity
/// </summary>
public class InternetConnectivityHealthCheck : IHealthCheck
{
    private readonly ILogger<InternetConnectivityHealthCheck> _logger;

    public InternetConnectivityHealthCheck(ILogger<InternetConnectivityHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking internet connectivity");

        try
        {
            using var ping = new Ping();
            var reply = ping.Send("8.8.8.8", 2000);

            if (reply.Status == IPStatus.Success)
            {
                _logger.LogInformation("Internet connectivity check successful");
                return Task.FromResult(HealthCheckResult.Healthy("Internet connection is available"));
            }
            else
            {
                var message = $"Ping failed. Status: {reply.Status}";
                _logger.LogWarning("Internet connectivity check failed: {Message}", message);
                return Task.FromResult(HealthCheckResult.Degraded(message));
            }
        }
        catch (PingException pex)
        {
            _logger.LogError(pex, "Internet connectivity check failed with PingException");
            return Task.FromResult(HealthCheckResult.Unhealthy($"Ping failed: {pex.Message}", pex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internet connectivity check failed with unexpected exception");
            return Task.FromResult(HealthCheckResult.Unhealthy($"Unexpected error during internet check: {ex.Message}", ex));
        }
    }
}
