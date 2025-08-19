using Microsoft.AspNetCore.Mvc;
using VictorianTranslator.Services;

namespace VictorianTranslator.Server.Controllers;

/// <summary>
/// Controller for health check endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDiagnosticService _diagnosticService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IDiagnosticService diagnosticService, ILogger<HealthController> logger)
    {
        _diagnosticService = diagnosticService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint that reports the connection status of all external dependencies
    /// </summary>
    /// <returns>Health check results including status of databases, APIs, and other dependencies</returns>
    [HttpGet]
    [Route("/health")]
    [Route("/healthz")]
    public async Task<IActionResult> Health()
    {
        try
        {
            _logger.LogInformation("Health check requested");
            
            var diagnosticResults = await _diagnosticService.RunChecksAsync();
            
            var overallHealthy = diagnosticResults.All(r => r.Success);
            var status = overallHealthy ? "Healthy" : "Unhealthy";
            
            var response = new
            {
                Status = status,
                Timestamp = DateTime.UtcNow,
                Checks = diagnosticResults.Select(r => new
                {
                    Name = r.CheckName,
                    Status = r.Success ? "Healthy" : "Unhealthy",
                    Description = r.Message,
                    Error = r.Error?.Message
                })
            };

            _logger.LogInformation("Health check completed with status: {Status}", status);
            
            return overallHealthy ? Ok(response) : StatusCode(503, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            
            var errorResponse = new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = "Health check failed",
                Message = ex.Message
            };
            
            return StatusCode(503, errorResponse);
        }
    }

    /// <summary>
    /// Simple liveness probe endpoint
    /// </summary>
    /// <returns>OK if the application is running</returns>
    [HttpGet]
    [Route("/health/live")]
    public IActionResult Live()
    {
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe endpoint that checks if the application is ready to serve traffic
    /// </summary>
    /// <returns>OK if the application is ready</returns>
    [HttpGet]
    [Route("/health/ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // Run a subset of critical checks for readiness
            var diagnosticResults = await _diagnosticService.RunChecksAsync();
            var criticalChecks = diagnosticResults.Where(r => r.CheckName.Contains("Configuration") || r.CheckName.Contains("Connection"));
            
            var ready = criticalChecks.All(r => r.Success);
            var status = ready ? "Ready" : "NotReady";
            
            var response = new
            {
                Status = status,
                Timestamp = DateTime.UtcNow,
                CriticalChecks = criticalChecks.Select(r => new
                {
                    Name = r.CheckName,
                    Status = r.Success ? "Healthy" : "Unhealthy",
                    Description = r.Message
                })
            };
            
            return ready ? Ok(response) : StatusCode(503, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed with exception");
            return StatusCode(503, new { Status = "NotReady", Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }
}
