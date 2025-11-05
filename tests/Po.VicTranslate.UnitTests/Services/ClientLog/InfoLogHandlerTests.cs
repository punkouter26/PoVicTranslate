using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Services.ClientLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.ClientLog;

public class InfoLogHandlerTests
{
    [Theory]
    [InlineData("info", true)]
    [InlineData("INFO", true)]
    [InlineData("information", true)]
    [InlineData("debug", true)]
    [InlineData("DEBUG", true)]
    [InlineData("error", false)]
    [InlineData("warning", false)]
    public void CanHandle_WithVariousLogLevels_ReturnsExpectedResult(string level, bool expected)
    {
        // Arrange
        var handler = new InfoLogHandler();
        var logEntry = new ClientLogEntry { Level = level };

        // Act
        var result = handler.CanHandle(logEntry);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task HandleAsync_WithInfoLog_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var telemetryConfig = TelemetryConfiguration.CreateDefault();
        var telemetryClient = new TelemetryClient(telemetryConfig);
        var handler = new InfoLogHandler();
        var logEntry = new ClientLogEntry
        {
            Level = "info",
            Message = "Test info message",
            Page = "/test",
            UserId = "user123"
        };

        // Act
        await handler.HandleAsync(logEntry, mockLogger.Object, telemetryClient);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test info message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDebugLog_LogsDebug()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var telemetryConfig = TelemetryConfiguration.CreateDefault();
        var telemetryClient = new TelemetryClient(telemetryConfig);
        var handler = new InfoLogHandler();
        var logEntry = new ClientLogEntry
        {
            Level = "debug",
            Message = "Test debug message",
            Page = "/test",
            UserId = "user123"
        };

        // Act
        await handler.HandleAsync(logEntry, mockLogger.Object, telemetryClient);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test debug message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
