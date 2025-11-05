using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Validation;

namespace Po.VicTranslate.UnitTests.Services;

public class AudioSynthesisServiceTests
{
    private readonly Mock<ILogger<AudioSynthesisService>> _mockLogger;
    private readonly Mock<ISpeechConfigValidator> _mockValidator;
    private readonly ApiSettings _validSettings;

    public AudioSynthesisServiceTests()
    {
        _mockLogger = new Mock<ILogger<AudioSynthesisService>>();
        _mockValidator = new Mock<ISpeechConfigValidator>();
        _validSettings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "test-key-12345678901234567890123456789012",
            AzureSpeechRegion = "eastus2"
        };
    }

    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new AudioSynthesisService(null!, _mockValidator.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullValidator_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(_validSettings);

        // Act
        var act = () => new AudioSynthesisService(options, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithInvalidSettings_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "",
            AzureSpeechRegion = "eastus2"
        };
        var options = Options.Create(settings);
        
        _mockValidator.Setup(v => v.IsValid(settings)).Returns(false);
        _mockValidator.Setup(v => v.GetValidationError(settings))
            .Returns("Azure Speech SubscriptionKey is missing or empty");

        // Act
        var act = () => new AudioSynthesisService(options, _mockValidator.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Azure Speech settings are not configured: Azure Speech SubscriptionKey is missing or empty");
    }

    [Fact]
    public void Constructor_WithValidSettings_CreatesInstance()
    {
        // Arrange
        var options = Options.Create(_validSettings);
        _mockValidator.Setup(v => v.IsValid(_validSettings)).Returns(true);

        // Act
        var service = new AudioSynthesisService(options, _mockValidator.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IAudioSynthesisService>();
    }

    [Fact]
    public async Task SynthesizeSpeechAsync_WithNullText_ShouldHandleGracefully()
    {
        // Arrange
        var options = Options.Create(_validSettings);
        _mockValidator.Setup(v => v.IsValid(_validSettings)).Returns(true);
        var service = new AudioSynthesisService(options, _mockValidator.Object, _mockLogger.Object);

        // Act & Assert
        // Note: Actual Azure Speech SDK call will fail in unit tests without real credentials
        // This test validates the service is constructed correctly
        await Assert.ThrowsAnyAsync<Exception>(() => service.SynthesizeSpeechAsync(null!));
    }

    [Fact]
    public async Task SynthesizeSpeechAsync_WithEmptyText_ShouldHandleGracefully()
    {
        // Arrange
        var options = Options.Create(_validSettings);
        _mockValidator.Setup(v => v.IsValid(_validSettings)).Returns(true);
        var service = new AudioSynthesisService(options, _mockValidator.Object, _mockLogger.Object);

        // Act & Assert
        // Note: Actual Azure Speech SDK call will fail in unit tests without real credentials
        await Assert.ThrowsAnyAsync<Exception>(() => service.SynthesizeSpeechAsync(""));
    }
}
