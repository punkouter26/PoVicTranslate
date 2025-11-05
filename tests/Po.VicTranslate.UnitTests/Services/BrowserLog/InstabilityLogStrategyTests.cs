using Xunit;
using FluentAssertions;
using Moq;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.BrowserLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.BrowserLog;

public class InstabilityLogStrategyTests
{
    [Theory]
    [InlineData("instability", true)]
    [InlineData("INSTABILITY", true)]
    [InlineData("Instability", true)]
    [InlineData("event", false)]
    [InlineData("failure", false)]
    [InlineData(null, false)]
    public void CanHandle_WithVariousLogTypes_ReturnsExpectedResult(string? logType, bool expected)
    {
        // Arrange
        var strategy = new InstabilityLogStrategy();

        // Act
        var result = strategy.CanHandle(logType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task LogAsync_WithValidRequest_CallsDebugServiceLogInstability()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new InstabilityLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "instability",
            Payload = new BrowserLogPayload
            {
                Component = "TestComponent",
                Issue = "Test issue",
                DiagnosticData = new { ErrorCode = 123 }
            }
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogInstabilityAsync("TestComponent", "Test issue", It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WithNullPayload_UsesDefaultValues()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new InstabilityLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "instability",
            Payload = null
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogInstabilityAsync("Browser", "Browser instability", null),
            Times.Once);
    }
}
