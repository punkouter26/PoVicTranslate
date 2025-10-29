using Microsoft.AspNetCore.Mvc;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Models;

namespace Po.VicTranslate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IDebugLogService _debugLogService;
    private readonly ILogger<DebugController> _logger;

    public DebugController(IDebugLogService debugLogService, ILogger<DebugController> logger)
    {
        _debugLogService = debugLogService;
        _logger = logger;
    }

    /// <summary>
    /// Get recent debug logs
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult<IEnumerable<DebugLogEntry>>> GetRecentLogs(
        [FromQuery] int count = 100,
        [FromQuery] string? eventType = null)
    {
        try
        {
            var logs = await _debugLogService.GetRecentLogsAsync(count, eventType);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent logs");
            return StatusCode(500, "Error retrieving logs");
        }
    }

    /// <summary>
    /// Generate and return a summary report
    /// </summary>
    [HttpGet("summary-report")]
    public async Task<ActionResult<DebugSummaryReport>> GetSummaryReport()
    {
        try
        {
            var report = await _debugLogService.GenerateSummaryReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary report");
            return StatusCode(500, "Error generating summary report");
        }
    }

    /// <summary>
    /// Log a test event (for debugging the debug system)
    /// </summary>
    [HttpPost("test-event")]
    public async Task<ActionResult> LogTestEvent([FromBody] TestEventRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            await _debugLogService.LogEventAsync(
                request.EventType ?? "Test",
                request.Message ?? "Test event logged via API",
                request.Data
            );
            return Ok(new { Message = "Test event logged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging test event");
            return StatusCode(500, "Error logging test event");
        }
    }

    /// <summary>
    /// Simulate an instability event (for testing)
    /// </summary>
    [HttpPost("test-instability")]
    public async Task<ActionResult> LogTestInstability([FromBody] TestInstabilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            await _debugLogService.LogInstabilityAsync(
                request.Component ?? "TestComponent",
                request.Issue ?? "Simulated instability issue",
                request.DiagnosticData
            );
            return Ok(new { Message = "Test instability logged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging test instability");
            return StatusCode(500, "Error logging test instability");
        }
    }

    /// <summary>
    /// Simulate a structural failure (for testing)
    /// </summary>
    [HttpPost("test-failure")]
    public async Task<ActionResult> LogTestFailure([FromBody] TestFailureRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            Exception? testException = null;
            if (request.IncludeException)
            {
                testException = new InvalidOperationException(request.ExceptionMessage ?? "Simulated test exception");
            }

            await _debugLogService.LogStructuralFailureAsync(
                request.Component ?? "TestComponent",
                request.Failure ?? "Simulated structural failure",
                testException,
                request.Context
            );
            return Ok(new { Message = "Test structural failure logged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging test structural failure");
            return StatusCode(500, "Error logging test structural failure");
        }
    }

    /// <summary>
    /// Clean up old debug logs
    /// </summary>
    [HttpDelete("cleanup")]
    public async Task<ActionResult> CleanupOldLogs([FromQuery] int retentionHours = 24)
    {
        try
        {
            var retentionPeriod = TimeSpan.FromHours(retentionHours);
            await _debugLogService.CleanupOldLogsAsync(retentionPeriod);
            return Ok(new { Message = $"Cleanup completed. Logs older than {retentionHours} hours removed." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
            return StatusCode(500, "Error during cleanup");
        }
    }

    /// <summary>
    /// Receive logs from browser-side JavaScript
    /// </summary>
    [HttpPost("browser-log")]
    public async Task<ActionResult> ReceiveBrowserLog([FromBody] BrowserLogRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            switch (request.Type?.ToLower())
            {
                case "event":
                case "browser-event":
                    await _debugLogService.LogEventAsync(
                        request.Payload?.EventType ?? "BrowserEvent",
                        request.Payload?.Message ?? "Browser event",
                        request.Payload?.Data
                    );
                    break;

                case "instability":
                    await _debugLogService.LogInstabilityAsync(
                        request.Payload?.Component ?? "Browser",
                        request.Payload?.Issue ?? "Browser instability",
                        request.Payload?.DiagnosticData
                    );
                    break;

                case "failure":
                    await _debugLogService.LogStructuralFailureAsync(
                        request.Payload?.Component ?? "Browser",
                        request.Payload?.Failure ?? "Browser failure",
                        null,
                        request.Payload?.Context
                    );
                    break;

                default:
                    await _debugLogService.LogEventAsync(
                        "UnknownBrowserEvent",
                        $"Unknown browser log type: {request.Type}",
                        request.Payload
                    );
                    break;
            }

            return Ok(new { Message = "Browser log received successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing browser log");
            return StatusCode(500, "Error processing browser log");
        }
    }

    /// <summary>
    /// Get debug system health status
    /// </summary>
    [HttpGet("health")]
    public Task<ActionResult> GetDebugHealth()
    {
        try
        {
            var debugPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "DEBUG");
            var logsPath = Path.Combine(debugPath, "logs");
            var reportsPath = Path.Combine(debugPath, "reports");

            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Paths = new
                {
                    DebugRoot = debugPath,
                    LogsPath = logsPath,
                    ReportsPath = reportsPath
                },
                DirectoryInfo = new
                {
                    DebugRootExists = Directory.Exists(debugPath),
                    LogsPathExists = Directory.Exists(logsPath),
                    ReportsPathExists = Directory.Exists(reportsPath),
                    LogFileCount = Directory.Exists(logsPath) ? Directory.GetFiles(logsPath, "*.json").Length : 0,
                    ReportFileCount = Directory.Exists(reportsPath) ? Directory.GetFiles(reportsPath, "*.json").Length : 0
                }
            };

            return Task.FromResult<ActionResult>(Ok(health));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking debug health");
            return Task.FromResult<ActionResult>(StatusCode(500, "Error checking debug health"));
        }
    }
}

// Request DTOs
public class TestEventRequest
{
    public string? EventType { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}

public class TestInstabilityRequest
{
    public string? Component { get; set; }
    public string? Issue { get; set; }
    public object? DiagnosticData { get; set; }
}

public class TestFailureRequest
{
    public string? Component { get; set; }
    public string? Failure { get; set; }
    public bool IncludeException { get; set; }
    public string? ExceptionMessage { get; set; }
    public object? Context { get; set; }
}

public class BrowserLogRequest
{
    public string? Type { get; set; }
    public BrowserLogPayload? Payload { get; set; }
}

public class BrowserLogPayload
{
    public string? EventType { get; set; }
    public string? Message { get; set; }
    public string? Component { get; set; }
    public string? Level { get; set; }
    public string? Issue { get; set; }
    public string? Failure { get; set; }
    public object? Data { get; set; }
    public object? DiagnosticData { get; set; }
    public object? Context { get; set; }
    public string? SessionId { get; set; }
    public string? Timestamp { get; set; }
    public string? Url { get; set; }
}
