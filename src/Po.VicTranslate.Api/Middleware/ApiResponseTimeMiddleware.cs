using System.Diagnostics;
using Po.VicTranslate.Api.Services;

namespace Po.VicTranslate.Api.Middleware;

/// <summary>
/// Middleware that tracks API response times and status codes for Application Insights monitoring.
/// </summary>
public class ApiResponseTimeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiResponseTimeMiddleware> _logger;

    public ApiResponseTimeMiddleware(RequestDelegate next, ILogger<ApiResponseTimeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Measures request duration and tracks response metrics via custom telemetry.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, ICustomTelemetryService telemetryService)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = $"{context.Request.Method} {context.Request.Path}";

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var durationMs = stopwatch.ElapsedMilliseconds;

            // Track the API response in Application Insights
            telemetryService.TrackApiResponse(endpoint, statusCode, durationMs);

            // Log slow requests (>2 seconds)
            if (durationMs > 2000)
            {
                _logger.LogWarning(
                    "Slow API request: {Endpoint} returned {StatusCode} in {DurationMs}ms",
                    endpoint,
                    statusCode,
                    durationMs);
            }
        }
    }
}
