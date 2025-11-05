using Microsoft.ApplicationInsights;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.ClientLog;

/// <summary>
/// Handler interface for processing client log entries using the Chain of Responsibility pattern.
/// Each handler processes specific log levels and passes unhandled entries to the next handler.
/// </summary>
public interface IClientLogHandler
{
    /// <summary>
    /// Determines if this handler can process the given log entry.
    /// </summary>
    /// <param name="logEntry">The client log entry to check</param>
    /// <returns>True if this handler can process the entry, false otherwise</returns>
    bool CanHandle(ClientLogEntry logEntry);

    /// <summary>
    /// Processes the client log entry by logging to server infrastructure and Application Insights.
    /// </summary>
    /// <param name="logEntry">The client log entry to process</param>
    /// <param name="logger">The server logger instance</param>
    /// <param name="telemetryClient">The Application Insights telemetry client</param>
    Task HandleAsync(ClientLogEntry logEntry, ILogger logger, TelemetryClient telemetryClient);
}
