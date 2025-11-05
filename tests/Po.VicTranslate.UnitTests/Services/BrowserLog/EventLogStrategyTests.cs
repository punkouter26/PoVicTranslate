using Xunit;
using FluentAssertions;
using Moq;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.BrowserLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.BrowserLog;

public class EventLogStrategyTests
{
    [Theory]
    [InlineData("event", true)]
    [InlineData("browser-event", true)]
    [InlineData("EVENT", true)]
    [InlineData("Browser-Event", true)]
    [InlineData("instability", false)]
    [InlineData("failure", false)]
    [InlineData(null, false)]
    public void CanHandle_WithVariousLogTypes_ReturnsExpectedResult(string? logType, bool expected)
    {
        // Arrange
        var strategy = new EventLogStrategy();

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
        var strategy = new EventLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "event",
            Payload = new BrowserLogPayload
            {
                EventType = "TestEvent",
                Message = "Test message",
                Data = new { TestProp = "TestValue" }
            }
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogEventAsync("TestEvent", "Test message", It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WithNullPayload_UsesDefaultValues()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new EventLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "event",
            Payload = null
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogEventAsync("BrowserEvent", "Browser event", null),
            Times.Once);
    }
}
