using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Services;

public interface IDebugLogService
{
    /// <summary>
    /// Logs initialization events (app startup, service registration, etc.)
    /// </summary>
    Task LogInitializationAsync(string component, string message, object? data = null);

    /// <summary>
    /// Logs application events (user actions, API calls, etc.)
    /// </summary>
    Task LogEventAsync(string eventType, string message, object? data = null);

    /// <summary>
    /// Logs instability or error detection events
    /// </summary>
    Task LogInstabilityAsync(string component, string issue, object? diagnosticData = null);

    /// <summary>
    /// Logs structural failures or critical errors
    /// </summary>
    Task LogStructuralFailureAsync(string component, string failure, Exception? exception = null, object? context = null);

    /// <summary>
    /// Generates a summary report of application state
    /// </summary>
    Task<DebugSummaryReport> GenerateSummaryReportAsync();

    /// <summary>
    /// Gets recent debug log entries
    /// </summary>
    Task<IEnumerable<DebugLogEntry>> GetRecentLogsAsync(int count = 100, string? eventType = null);

    /// <summary>
    /// Clears old debug logs (keeps recent entries)
    /// </summary>
    Task CleanupOldLogsAsync(TimeSpan retentionPeriod);
}
