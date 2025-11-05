using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Po.VicTranslate.Api.Middleware;
using Po.VicTranslate.Api.Services;
using Xunit;

namespace Po.VicTranslate.UnitTests.Middleware;

public class ApiResponseTimeMiddlewareTests
{
    private readonly Mock<ILogger<ApiResponseTimeMiddleware>> _mockLogger;
    private readonly Mock<ICustomTelemetryService> _mockTelemetry;
    private readonly Mock<RequestDelegate> _mockNext;

    public ApiResponseTimeMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ApiResponseTimeMiddleware>>();
        _mockTelemetry = new Mock<ICustomTelemetryService>();
        _mockNext = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_TracksSuccessfulRequest()
    {
        // Arrange
        var middleware = new ApiResponseTimeMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/translation";
        context.Response.StatusCode = 200;

        _mockNext.Setup(next => next(context))
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, _mockTelemetry.Object);

        // Assert
        _mockTelemetry.Verify(
            t => t.TrackApiResponse(
                "GET /api/translation",
                200,
                It.IsAny<long>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_TracksErrorResponse()
    {
        // Arrange
        var middleware = new ApiResponseTimeMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/lyrics";
        context.Response.StatusCode = 500;

        _mockNext.Setup(next => next(context))
            .Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, _mockTelemetry.Object);

        // Assert
        _mockTelemetry.Verify(
            t => t.TrackApiResponse(
                "POST /api/lyrics",
                500,
                It.IsAny<long>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_TracksDurationCorrectly()
    {
        // Arrange
        var middleware = new ApiResponseTimeMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/health";
        context.Response.StatusCode = 200;

        long capturedDuration = 0;
        _mockTelemetry.Setup(t => t.TrackApiResponse(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
            .Callback<string, int, long>((_, _, duration) => capturedDuration = duration);

        _mockNext.Setup(next => next(context))
            .Returns(async () =>
            {
                await Task.Delay(100); // Simulate 100ms processing
            });

        // Act
        await middleware.InvokeAsync(context, _mockTelemetry.Object);

        // Assert
        Assert.True(capturedDuration >= 100, $"Duration should be at least 100ms, but was {capturedDuration}ms");
        Assert.True(capturedDuration < 200, $"Duration should be less than 200ms, but was {capturedDuration}ms");
    }

    [Fact]
    public async Task InvokeAsync_LogsSlowRequests()
    {
        // Arrange
        var middleware = new ApiResponseTimeMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/slow-endpoint";
        context.Response.StatusCode = 200;

        _mockNext.Setup(next => next(context))
            .Returns(async () =>
            {
                await Task.Delay(2100); // Simulate slow request (>2000ms)
            });

        // Act
        await middleware.InvokeAsync(context, _mockTelemetry.Object);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow API request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotLogFastRequests()
    {
        // Arrange
        var middleware = new ApiResponseTimeMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/fast-endpoint";
        context.Response.StatusCode = 200;

        _mockNext.Setup(next => next(context))
            .Returns(Task.CompletedTask); // Fast request (<2000ms)

        // Act
        await middleware.InvokeAsync(context, _mockTelemetry.Object);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow API request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_TracksEvenWhenNextThrowsException()
    {
        // Arrange
        var middleware = new ApiResponseTimeMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/error";
        context.Response.StatusCode = 500; // Set by exception handler

        _mockNext.Setup(next => next(context))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await middleware.InvokeAsync(context, _mockTelemetry.Object));

        // Telemetry should still be tracked in finally block
        _mockTelemetry.Verify(
            t => t.TrackApiResponse(
                "GET /api/error",
                It.IsAny<int>(),
                It.IsAny<long>()),
            Times.Once);
    }

    [Theory]
    [InlineData("GET", "/api/lyrics")]
    [InlineData("POST", "/api/translation")]
    [InlineData("DELETE", "/api/lyrics/management/songs/123")]
    [InlineData("PUT", "/api/speech")]
    public async Task InvokeAsync_TracksVariousEndpoints(string method, string path)
    {
        // Arrange
        var middleware = new ApiResponseTimeMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.StatusCode = 200;

        _mockNext.Setup(next => next(context))
            .Returns(Task.CompletedTask);

        var expectedEndpoint = $"{method} {path}";

        // Act
        await middleware.InvokeAsync(context, _mockTelemetry.Object);

        // Assert
        _mockTelemetry.Verify(
            t => t.TrackApiResponse(
                expectedEndpoint,
                200,
                It.IsAny<long>()),
            Times.Once);
    }
}
