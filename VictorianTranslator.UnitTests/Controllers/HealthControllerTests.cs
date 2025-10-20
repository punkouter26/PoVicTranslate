using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VictorianTranslator.Server.Controllers;
using VictorianTranslator.Models;
using VictorianTranslator.Services;
using Xunit;

namespace VictorianTranslator.UnitTests.Controllers;

public class HealthControllerTests
{
    private readonly Mock<IDiagnosticService> _mockDiagnosticService;
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockDiagnosticService = new Mock<IDiagnosticService>();
        _mockLogger = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_mockDiagnosticService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Health_ShouldReturnOkWithDiagnosticResults()
    {
        // Arrange
        var expectedResults = new List<DiagnosticResult>
        {
            new() { CheckName = "Test Service", Success = true, Message = "OK" }
        };

        _mockDiagnosticService
            .Setup(x => x.RunChecksAsync())
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.Health();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Health_ShouldCallDiagnosticService()
    {
        // Arrange
        _mockDiagnosticService
            .Setup(x => x.RunChecksAsync())
            .ReturnsAsync(new List<DiagnosticResult>());

        // Act
        await _controller.Health();

        // Assert
        _mockDiagnosticService.Verify(
            x => x.RunChecksAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Health_WhenServiceThrows_ShouldReturnServiceUnavailable()
    {
        // Arrange
        _mockDiagnosticService
            .Setup(x => x.RunChecksAsync())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        // Act
        var result = await _controller.Health();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(503);
    }
}
