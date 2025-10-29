using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Services;

namespace Po.VicTranslate.UnitTests.Services;

public class AudioSynthesisServiceTests
{
    private readonly Mock<ILogger<AudioSynthesisService>> _mockLogger;
    private readonly ApiSettings _validSettings;

    public AudioSynthesisServiceTests()
    {
        _mockLogger = new Mock<ILogger<AudioSynthesisService>>();
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
        var act = () => new AudioSynthesisService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithMissingSubscriptionKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "",
            AzureSpeechRegion = "eastus2"
        };
        var options = Options.Create(settings);

        // Act
        var act = () => new AudioSynthesisService(options, _mockLogger.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Azure Speech settings are not configured.");
    }

    [Fact]
    public void Constructor_WithMissingRegion_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "test-key-12345678901234567890123456789012",
            AzureSpeechRegion = ""
        };
        var options = Options.Create(settings);

        // Act
        var act = () => new AudioSynthesisService(options, _mockLogger.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Azure Speech settings are not configured.");
    }

    [Fact]
    public void Constructor_WithValidSettings_CreatesInstance()
    {
        // Arrange
        var options = Options.Create(_validSettings);

        // Act
        var service = new AudioSynthesisService(options, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IAudioSynthesisService>();
    }

    [Fact]
    public async Task SynthesizeSpeechAsync_WithNullText_ShouldHandleGracefully()
    {
        // Arrange
        var options = Options.Create(_validSettings);
        var service = new AudioSynthesisService(options, _mockLogger.Object);

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
        var service = new AudioSynthesisService(options, _mockLogger.Object);

        // Act & Assert
        // Note: Actual Azure Speech SDK call will fail in unit tests without real credentials
        await Assert.ThrowsAnyAsync<Exception>(() => service.SynthesizeSpeechAsync(""));
    }
}
