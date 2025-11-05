using Po.VicTranslate.Api.Services;

namespace Po.VicTranslate.Api.BackgroundServices;

/// <summary>
/// Background service that automatically cleans up old debug logs and reports.
/// Runs every 6 hours and removes logs older than 24 hours.
/// </summary>
public class DebugLogCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DebugLogCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6);
    private readonly TimeSpan _retentionPeriod = TimeSpan.FromHours(24);

    public DebugLogCleanupService(
        IServiceProvider serviceProvider,
        ILogger<DebugLogCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Debug log cleanup service started. Running every {Interval} hours, " +
            "removing logs older than {Retention} hours.", 
            _cleanupInterval.TotalHours, 
            _retentionPeriod.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for the cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);

                // Create a scope to get the debug log service
                using var scope = _serviceProvider.CreateScope();
                var debugLogService = scope.ServiceProvider.GetRequiredService<IDebugLogService>();

                _logger.LogInformation("Starting automatic debug log cleanup...");

                // Perform cleanup
                await debugLogService.CleanupOldLogsAsync(_retentionPeriod);

                _logger.LogInformation("Debug log cleanup completed successfully.");
            }
            catch (OperationCanceledException)
            {
                // Service is shutting down
                _logger.LogInformation("Debug log cleanup service is shutting down.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during debug log cleanup. " +
                    "Will retry in {Interval} hours.", _cleanupInterval.TotalHours);
                // Continue running - don't crash the service on cleanup errors
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Debug log cleanup service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}
