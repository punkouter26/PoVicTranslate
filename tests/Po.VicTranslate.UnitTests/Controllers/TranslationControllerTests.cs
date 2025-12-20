using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Controllers;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Validation;
using Xunit;

namespace Po.VicTranslate.UnitTests.Controllers;

public class TranslationControllerTests
{
    private readonly Mock<ITranslationService> _mockTranslationService;
    private readonly Mock<IAudioSynthesisService> _mockAudioSynthesisService;
    private readonly Mock<ICustomTelemetryService> _mockTelemetryService;
    private readonly Mock<IInputValidator> _mockInputValidator;
    private readonly Mock<ILogger<TranslationController>> _mockLogger;
    private readonly TranslationController _controller;

    public TranslationControllerTests()
    {
        _mockTranslationService = new Mock<ITranslationService>();
        _mockAudioSynthesisService = new Mock<IAudioSynthesisService>();
        _mockTelemetryService = new Mock<ICustomTelemetryService>();
        _mockInputValidator = new Mock<IInputValidator>();
        _mockLogger = new Mock<ILogger<TranslationController>>();

        _controller = new TranslationController(
            _mockTranslationService.Object,
            _mockAudioSynthesisService.Object,
            _mockTelemetryService.Object,
            _mockInputValidator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Translate_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = new TranslationRequest { Text = "Hello world" };
        var expectedTranslation = "Good morrow, esteemed fellow denizens of this terrestrial sphere";

        // Setup validator to return valid result with sanitized text
        _mockInputValidator
            .Setup(x => x.ValidateTextContent(request.Text, 5000))
            .Returns(new ValidationResult
            {
                IsValid = true,
                SanitizedValue = request.Text
            });

        _mockTranslationService
            .Setup(x => x.TranslateToVictorianEnglishAsync(request.Text))
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

        // Setup validator to return failure for empty text
        _mockInputValidator
            .Setup(x => x.ValidateTextContent(text!, 5000))
            .Returns(ValidationResult.Failure("Text content cannot be empty."));

        // Act
        var result = await _controller.Translate(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
    }

    [Fact]
    public async Task Translate_ShouldCallTranslationService()
    {
        // Arrange
        var request = new TranslationRequest { Text = "Test text" };

        // Setup validator to return valid result
        _mockInputValidator
            .Setup(x => x.ValidateTextContent(request.Text, 5000))
            .Returns(new ValidationResult
            {
                IsValid = true,
                SanitizedValue = request.Text
            });

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

        // Setup validator to return valid result
        _mockInputValidator
            .Setup(x => x.ValidateTextContent(request.Text, 5000))
            .Returns(new ValidationResult
            {
                IsValid = true,
                SanitizedValue = request.Text
            });

        _mockTranslationService
            .Setup(x => x.TranslateToVictorianEnglishAsync(request.Text))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Translate(request));
    }
}
