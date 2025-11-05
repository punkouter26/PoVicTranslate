using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Services.ClientLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.ClientLog;

public class WarningLogHandlerTests
{
    [Theory]
    [InlineData("warning", true)]
    [InlineData("WARNING", true)]
    [InlineData("Warning", true)]
    [InlineData("error", false)]
    [InlineData("info", false)]
    [InlineData(null, false)]
    public void CanHandle_WithVariousLogLevels_ReturnsExpectedResult(string? level, bool expected)
    {
        // Arrange
        var handler = new WarningLogHandler();
        var logEntry = new ClientLogEntry { Level = level ?? "Info" };

        // Act
        var result = handler.CanHandle(logEntry);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task HandleAsync_WithValidWarningLog_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var telemetryConfig = TelemetryConfiguration.CreateDefault();
        var telemetryClient = new TelemetryClient(telemetryConfig);
        var handler = new WarningLogHandler();
        var logEntry = new ClientLogEntry
        {
            Level = "warning",
            Message = "Test warning message",
            Page = "/test",
            UserId = "user123"
        };

        // Act
        await handler.HandleAsync(logEntry, mockLogger.Object, telemetryClient);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test warning message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
