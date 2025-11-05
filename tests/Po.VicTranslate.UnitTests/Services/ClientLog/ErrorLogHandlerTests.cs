using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Services.ClientLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.ClientLog;

public class ErrorLogHandlerTests
{
    [Theory]
    [InlineData("error", true)]
    [InlineData("ERROR", true)]
    [InlineData("Error", true)]
    [InlineData("warning", false)]
    [InlineData("info", false)]
    [InlineData(null, false)]
    public void CanHandle_WithVariousLogLevels_ReturnsExpectedResult(string? level, bool expected)
    {
        // Arrange
        var handler = new ErrorLogHandler();
        var logEntry = new ClientLogEntry { Level = level ?? "Info" };

        // Act
        var result = handler.CanHandle(logEntry);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task HandleAsync_WithValidErrorLog_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var telemetryConfig = TelemetryConfiguration.CreateDefault();
        var telemetryClient = new TelemetryClient(telemetryConfig);
        var handler = new ErrorLogHandler();
        var logEntry = new ClientLogEntry
        {
            Level = "error",
            Message = "Test error message",
            Page = "/test",
            UserId = "user123"
        };

        // Act
        await handler.HandleAsync(logEntry, mockLogger.Object, telemetryClient);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test error message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullUserId_UsesAnonymous()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var telemetryConfig = TelemetryConfiguration.CreateDefault();
        var telemetryClient = new TelemetryClient(telemetryConfig);
        var handler = new ErrorLogHandler();
        var logEntry = new ClientLogEntry
        {
            Level = "error",
            Message = "Test message",
            Page = "/test",
            UserId = null
        };

        // Act
        await handler.HandleAsync(logEntry, mockLogger.Object, telemetryClient);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Anonymous")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
