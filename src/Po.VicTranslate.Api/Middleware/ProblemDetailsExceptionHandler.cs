using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Po.VicTranslate.Api.Middleware;

/// <summary>
/// Global exception handler that transforms all exceptions into RFC 7807 Problem Details responses.
/// REQUIRED: Never expose raw exception messages or stack traces to clients.
/// </summary>
public sealed class ProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public ProblemDetailsExceptionHandler(
        ILogger<ProblemDetailsExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        // Log the full exception with stack trace internally using Serilog
        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            httpContext.TraceIdentifier,
            httpContext.Request.Path,
            httpContext.Request.Method);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        // Map exception types to appropriate status codes and user-safe messages
        var (statusCode, title, detail) = exception switch
        {
            ArgumentException or ArgumentNullException =>
                (HttpStatusCode.BadRequest, "Invalid Request", "The request contains invalid parameters."),

            KeyNotFoundException or FileNotFoundException =>
                (HttpStatusCode.NotFound, "Resource Not Found", "The requested resource was not found."),

            UnauthorizedAccessException =>
                (HttpStatusCode.Forbidden, "Access Denied", "You do not have permission to access this resource."),

            InvalidOperationException =>
                (HttpStatusCode.Conflict, "Operation Failed", "The requested operation could not be completed."),

            NotImplementedException =>
                (HttpStatusCode.NotImplemented, "Not Implemented", "This feature is not yet implemented."),

            TimeoutException =>
                (HttpStatusCode.RequestTimeout, "Request Timeout", "The request took too long to complete."),

            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        // ANTI-PATTERN: Never include exception.Message or exception.StackTrace in production
        // Only add debug info in Development environment for troubleshooting
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["exceptionMessage"] = exception.Message;
            // Stack trace helps local debugging but must NEVER appear in production
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        // Always include TraceId for correlation with logs
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return problemDetails;
    }
}
