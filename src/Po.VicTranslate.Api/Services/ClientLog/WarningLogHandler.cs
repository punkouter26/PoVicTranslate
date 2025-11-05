using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.Api.Services.ClientLog;

/// <summary>
/// Handler for warning-level client log entries.
/// Logs warnings with medium priority for potential issues that don't prevent functionality.
/// </summary>
public class WarningLogHandler : IClientLogHandler
{
    /// <inheritdoc />
    public bool CanHandle(ClientLogEntry logEntry)
    {
        return logEntry.Level.Equals("warning", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task HandleAsync(ClientLogEntry logEntry, ILogger logger, TelemetryClient telemetryClient)
    {
        // Log to server with Warning level
        logger.LogWarning("[CLIENT] {Message} | Page: {Page} | User: {User}",
            logEntry.Message, logEntry.Page, logEntry.UserId ?? "Anonymous");

        // Track as warning event in Application Insights
        var telemetry = new EventTelemetry("ClientLog")
        {
            Timestamp = logEntry.Timestamp
        };

        telemetry.Properties["Level"] = "Warning";
        telemetry.Properties["Severity"] = SeverityLevel.Warning.ToString();
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
