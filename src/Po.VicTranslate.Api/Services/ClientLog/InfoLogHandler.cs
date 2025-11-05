using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.ClientLog;

/// <summary>
/// Handler for info and debug-level client log entries.
/// Logs informational messages with standard priority for general application flow tracking.
/// </summary>
public class InfoLogHandler : IClientLogHandler
{
    /// <inheritdoc />
    public bool CanHandle(ClientLogEntry logEntry)
    {
        var level = logEntry.Level.ToLowerInvariant();
        return level == "info" || level == "debug" || level == "information";
    }

    /// <inheritdoc />
    public Task HandleAsync(ClientLogEntry logEntry, ILogger logger, TelemetryClient telemetryClient)
    {
        // Determine appropriate log level
        var logLevel = logEntry.Level.ToLowerInvariant() == "debug"
            ? LogLevel.Debug
            : LogLevel.Information;

        // Log to server
        logger.Log(logLevel, "[CLIENT] {Message} | Page: {Page} | User: {User}",
            logEntry.Message, logEntry.Page, logEntry.UserId ?? "Anonymous");

        // Track as informational event in Application Insights
        var telemetry = new EventTelemetry("ClientLog")
        {
            Timestamp = logEntry.Timestamp
        };

        telemetry.Properties["Level"] = logEntry.Level;
        telemetry.Properties["Severity"] = SeverityLevel.Information.ToString();
        telemetry.Properties["Message"] = logEntry.Message;
        telemetry.Properties["Page"] = logEntry.Page ?? "Unknown";
        telemetry.Properties["UserId"] = logEntry.UserId ?? "Anonymous";
        telemetry.Properties["UserAgent"] = logEntry.UserAgent ?? "Unknown";

        AddAdditionalData(logEntry, telemetry);

        telemetryClient.TrackEvent(telemetry);

        return Task.CompletedTask;
    }

    private static void AddAdditionalData(ClientLogEntry logEntry, EventTelemetry telemetry)
    {
        if (logEntry.AdditionalData == null) return;

        foreach (var kvp in logEntry.AdditionalData)
        {
            telemetry.Properties[$"Data_{kvp.Key}"] = kvp.Value?.ToString() ?? "";
        }
    }
}
