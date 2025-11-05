using Xunit;
using FluentAssertions;
using Moq;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.BrowserLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.BrowserLog;

public class FailureLogStrategyTests
{
    [Theory]
    [InlineData("failure", true)]
    [InlineData("FAILURE", true)]
    [InlineData("Failure", true)]
    [InlineData("event", false)]
    [InlineData("instability", false)]
    [InlineData(null, false)]
    public void CanHandle_WithVariousLogTypes_ReturnsExpectedResult(string? logType, bool expected)
    {
        // Arrange
        var strategy = new FailureLogStrategy();

        // Act
        var result = strategy.CanHandle(logType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task LogAsync_WithValidRequest_CallsDebugServiceLogStructuralFailure()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new FailureLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "failure",
            Payload = new BrowserLogPayload
            {
                Component = "TestComponent",
                Failure = "Test failure",
                Context = new { FailureType = "Critical" }
            }
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogStructuralFailureAsync("TestComponent", "Test failure", null, It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WithNullPayload_UsesDefaultValues()
    {
        // Arrange
        var mockDebugService = new Mock<IDebugLogService>();
        var strategy = new FailureLogStrategy();
        var request = new BrowserLogRequest
        {
            Type = "failure",
            Payload = null
        };

        // Act
        await strategy.LogAsync(request, mockDebugService.Object);

        // Assert
        mockDebugService.Verify(
            s => s.LogStructuralFailureAsync("Browser", "Browser failure", null, null),
            Times.Once);
    }
}
