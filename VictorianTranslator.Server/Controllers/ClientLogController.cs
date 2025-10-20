using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace VictorianTranslator.Server.Controllers;

/// <summary>
/// Controller for receiving client-side logs
/// </summary>
[ApiController]
[Route("api/log")]
public class ClientLogController : ControllerBase
{
    private readonly ILogger<ClientLogController> _logger;
    private readonly TelemetryClient _telemetryClient;

    public ClientLogController(ILogger<ClientLogController> logger, TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    /// <summary>
    /// Receives client-side log messages and forwards them to server logging infrastructure
    /// </summary>
    /// <param name="logEntry">The log entry from the client</param>
    [HttpPost("client")]
    public IActionResult PostClientLog([FromBody] ClientLogEntry logEntry)
    {
        try
        {
            // Log to server logger with appropriate level
            var logLevel = logEntry.Level.ToLowerInvariant() switch
            {
                "error" => LogLevel.Error,
                "warning" => LogLevel.Warning,
                "info" => LogLevel.Information,
                "debug" => LogLevel.Debug,
                _ => LogLevel.Information
            };

            _logger.Log(logLevel, "[CLIENT] {Message} | Page: {Page} | User: {User}",
                logEntry.Message, logEntry.Page, logEntry.UserId ?? "Anonymous");

            // Send to Application Insights as custom event
            var telemetry = new EventTelemetry("ClientLog")
            {
                Timestamp = logEntry.Timestamp
            };

            telemetry.Properties["Level"] = logEntry.Level;
            telemetry.Properties["Message"] = logEntry.Message;
            telemetry.Properties["Page"] = logEntry.Page ?? "Unknown";
            telemetry.Properties["UserId"] = logEntry.UserId ?? "Anonymous";
            telemetry.Properties["UserAgent"] = logEntry.UserAgent ?? "Unknown";

            if (logEntry.AdditionalData != null)
            {
                foreach (var kvp in logEntry.AdditionalData)
                {
                    telemetry.Properties[$"Data_{kvp.Key}"] = kvp.Value?.ToString() ?? "";
                }
            }

            _telemetryClient.TrackEvent(telemetry);

            return Ok(new { Success = true, Message = "Log received" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing client log");
            return StatusCode(500, new { Success = false, Message = "Error processing log" });
        }
    }
}

/// <summary>
/// Represents a log entry from the client
/// </summary>
public class ClientLogEntry
{
    public string Level { get; set; } = "Info";
    public string Message { get; set; } = string.Empty;
    public string? Page { get; set; }
    public string? UserId { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
