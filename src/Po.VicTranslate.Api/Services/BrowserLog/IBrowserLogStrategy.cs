using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.BrowserLog;

/// <summary>
/// Strategy interface for processing different types of browser logs.
/// Implements the Strategy Pattern to eliminate complex switch statements.
/// </summary>
public interface IBrowserLogStrategy
{
    /// <summary>
    /// Determines if this strategy can handle the given log type.
    /// </summary>
    /// <param name="logType">The type of browser log (e.g., "event", "instability", "failure")</param>
    /// <returns>True if this strategy handles the log type; otherwise, false.</returns>
    bool CanHandle(string? logType);

    /// <summary>
    /// Processes the browser log request using the debug log service.
    /// </summary>
    /// <param name="request">The browser log request to process</param>
    /// <param name="debugService">The debug log service for storing logs</param>
    Task LogAsync(BrowserLogRequest request, IDebugLogService debugService);
}
