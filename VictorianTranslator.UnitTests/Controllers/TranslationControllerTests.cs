using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VictorianTranslator.Models;
using VictorianTranslator.Server.Controllers;
using VictorianTranslator.Services;
using Xunit;

namespace VictorianTranslator.UnitTests.Controllers;

public class TranslationControllerTests
{
    private readonly Mock<ITranslationService> _mockTranslationService;
    private readonly TranslationController _controller;

    public TranslationControllerTests()
    {
        _mockTranslationService = new Mock<ITranslationService>();
        _controller = new TranslationController(_mockTranslationService.Object);
    }

    [Fact]
    public async Task Translate_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = new TranslationRequest { Text = "Hello world" };
        var expectedTranslation = "Good morrow, esteemed fellow denizens of this terrestrial sphere";
        
        _mockTranslationService
            .Setup(x => x.TranslateToVictorianEnglishAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedTranslation);

        // Act
        var result = await _controller.Translate(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as TranslationResponse;
        response.Should().NotBeNull();
        response!.TranslatedText.Should().Be(expectedTranslation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Translate_WithEmptyText_ShouldReturnBadRequest(string? text)
    {
        // Arrange
        var request = new TranslationRequest { Text = text! };

        // Act
        var result = await _controller.Translate(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Text cannot be empty.");
    }

    [Fact]
    public async Task Translate_ShouldCallTranslationService()
    {
        // Arrange
        var request = new TranslationRequest { Text = "Test text" };
        _mockTranslationService
            .Setup(x => x.TranslateToVictorianEnglishAsync(request.Text))
            .ReturnsAsync("Translated text");

        // Act
        await _controller.Translate(request);

        // Assert
        _mockTranslationService.Verify(
            x => x.TranslateToVictorianEnglishAsync(request.Text),
            Times.Once);
    }

    [Fact]
    public async Task Translate_WhenServiceThrowsException_ShouldPropagate()
    {
        // Arrange
        var request = new TranslationRequest { Text = "Test" };
        _mockTranslationService
            .Setup(x => x.TranslateToVictorianEnglishAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Translate(request));
    }
}
