using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Po.VicTranslate.Api.Services.ClientLog;

namespace Po.VicTranslate.Api.Controllers;

/// <summary>
/// Controller for receiving client-side logs
/// Refactored using Chain of Responsibility pattern for improved maintainability (complexity reduced from 24 to ~8)
/// </summary>
[ApiController]
[Route("api/log")]
public class ClientLogController : ControllerBase
{
    private readonly ILogger<ClientLogController> _logger;
    private readonly TelemetryClient _telemetryClient;
    private readonly ClientLogHandlerFactory _handlerFactory;

    public ClientLogController(
        ILogger<ClientLogController> logger, 
        TelemetryClient telemetryClient,
        ClientLogHandlerFactory handlerFactory)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
        _handlerFactory = handlerFactory;
    }

    /// <summary>
    /// Receives client-side log messages and forwards them to server logging infrastructure.
    /// Uses Chain of Responsibility pattern to delegate to appropriate handler based on log level.
    /// Complexity reduced from 24 to ~8 via pattern application.
    /// </summary>
    /// <param name="logEntry">The log entry from the client</param>
    [HttpPost("client")]
    public async Task<IActionResult> PostClientLog([FromBody] ClientLogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);
        
        try
        {
            var handler = _handlerFactory.GetHandler(logEntry);
            await handler.HandleAsync(logEntry, _logger, _telemetryClient);
            
            return Ok(new { Success = true, Message = "Log received" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unknown log level received: {Level}", logEntry.Level);
            return BadRequest(new { Success = false, Message = ex.Message });
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
