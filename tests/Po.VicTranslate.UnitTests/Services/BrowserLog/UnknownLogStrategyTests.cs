using Xunit;
using FluentAssertions;
using Moq;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.BrowserLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.BrowserLog;

public class UnknownLogStrategyTests
{
    [Theory]
    [InlineData("unknown-type", true)]
    [InlineData("event", true)]
    [InlineData("instability", true)]
    [InlineData("failure", true)]
    [InlineData(null, true)]
    public void CanHandle_WithAnyLogType_AlwaysReturnsTrue(string? logType, bool expected)
    {
        // Arrange
        var strategy = new UnknownLogStrategy();

        // Act
        var result = strategy.CanHandle(logType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task LogAsync_WithValidRequest_CallsDebugServiceLogEvent()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new UnknownLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "unknown-type",
            Payload = new BrowserLogPayload
            {
                Message = "Test message",
                Data = new { TestProp = "TestValue" }
            }
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogEventAsync("UnknownBrowserEvent", It.Is<string>(m => m.Contains("unknown-type")), It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WithNullLogType_LogsEmptyTypeString()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new UnknownLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = null,
            Payload = new BrowserLogPayload
            {
                Message = "Test message"
            }
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogEventAsync("UnknownBrowserEvent", "Unknown browser log type: ", It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WithNullPayload_PassesNullData()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new UnknownLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "unknown-type",
            Payload = null
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogEventAsync("UnknownBrowserEvent", "Unknown browser log type: unknown-type", null),
            Times.Once);
    }
}
