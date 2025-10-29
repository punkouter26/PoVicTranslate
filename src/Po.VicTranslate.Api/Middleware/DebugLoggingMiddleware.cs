using Po.VicTranslate.Api.Services;
using System.Diagnostics;
using System.Text;

namespace Po.VicTranslate.Api.Middleware;

public class DebugLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DebugLoggingMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DebugLoggingMiddleware(RequestDelegate next, ILogger<DebugLoggingMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var debugLogService = scope.ServiceProvider.GetRequiredService<IDebugLogService>();

            // Log request start
            await LogRequestStart(debugLogService, context, requestId);

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Log request completion
            stopwatch.Stop();
            await LogRequestCompletion(debugLogService, context, requestId, stopwatch.ElapsedMilliseconds);

            // Copy the response back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            using var scope = _serviceProvider.CreateScope();
            var debugLogService = scope.ServiceProvider.GetRequiredService<IDebugLogService>();

            await debugLogService.LogStructuralFailureAsync(
                "HttpRequest",
                $"Request failed: {context.Request.Method} {context.Request.Path}",
                ex,
                new
                {
                    RequestId = requestId,
                    Method = context.Request.Method,
                    Path = context.Request.Path.ToString(),
                    QueryString = context.Request.QueryString.ToString(),
                    Duration = stopwatch.ElapsedMilliseconds,
                    Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
                }
            );

            throw;
        }
    }

    private async Task LogRequestStart(IDebugLogService debugLogService, HttpContext context, string requestId)
    {
        try
        {
            var requestData = new
            {
                RequestId = requestId,
                Method = context.Request.Method,
                Path = context.Request.Path.ToString(),
                QueryString = context.Request.QueryString.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Headers = context.Request.Headers
                    .Where(h => !IsSecuritySensitiveHeader(h.Key))
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                ContentType = context.Request.ContentType,
                ContentLength = context.Request.ContentLength
            };

            await debugLogService.LogEventAsync(
                "HttpRequestStart",
                $"{context.Request.Method} {context.Request.Path}",
                requestData
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request start");
        }
    }

    private async Task LogRequestCompletion(IDebugLogService debugLogService, HttpContext context, string requestId, long durationMs)
    {
        try
        {
            var responseData = new
            {
                RequestId = requestId,
                StatusCode = context.Response.StatusCode,
                Duration = durationMs,
                ContentType = context.Response.ContentType,
                ContentLength = context.Response.ContentLength,
                Headers = context.Response.Headers
                    .Where(h => !IsSecuritySensitiveHeader(h.Key))
                    .ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            var logLevel = context.Response.StatusCode >= 400 ? "Warning" : "Info";
            var eventType = context.Response.StatusCode >= 500 ? "HttpRequestError" : "HttpRequestComplete";

            if (context.Response.StatusCode >= 500)
            {
                await debugLogService.LogInstabilityAsync(
                    "HttpRequest",
                    $"Server error: {context.Request.Method} {context.Request.Path} returned {context.Response.StatusCode}",
                    responseData
                );
            }
            else if (context.Response.StatusCode >= 400)
            {
                await debugLogService.LogEventAsync(
                    "HttpRequestClientError",
                    $"Client error: {context.Request.Method} {context.Request.Path} returned {context.Response.StatusCode}",
                    responseData
                );
            }
            else
            {
                await debugLogService.LogEventAsync(
                    eventType,
                    $"{context.Request.Method} {context.Request.Path} completed in {durationMs}ms",
                    responseData
                );
            }

            // Log slow requests
            if (durationMs > 1000) // More than 1 second
            {
                await debugLogService.LogInstabilityAsync(
                    "HttpRequest",
                    $"Slow request detected: {context.Request.Method} {context.Request.Path} took {durationMs}ms",
                    responseData
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request completion");
        }
    }

    private static bool IsSecuritySensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "authorization",
            "cookie",
            "x-api-key",
            "x-auth-token"
        };

        return sensitiveHeaders.Contains(headerName.ToLowerInvariant());
    }
}
