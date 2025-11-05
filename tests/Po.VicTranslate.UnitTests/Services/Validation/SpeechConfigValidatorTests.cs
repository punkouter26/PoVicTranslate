using Xunit;
using FluentAssertions;
using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Services.Validation;

namespace Po.VicTranslate.UnitTests.Services.Validation;

public class SpeechConfigValidatorTests
{
    private readonly SpeechConfigValidator _validator;

    public SpeechConfigValidatorTests()
    {
        _validator = new SpeechConfigValidator();
    }

    [Fact]
    public void IsValid_WithNullSettings_ReturnsFalse()
    {
        // Act
        var result = _validator.IsValid(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithValidSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "test-key-123",
            AzureSpeechRegion = "eastus2"
        };

        // Act
        var result = _validator.IsValid(settings);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMissingSubscriptionKey_ReturnsFalse()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "",
            AzureSpeechRegion = "eastus2"
        };

        // Act
        var result = _validator.IsValid(settings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingRegion_ReturnsFalse()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "test-key-123",
            AzureSpeechRegion = ""
        };

        // Act
        var result = _validator.IsValid(settings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceSubscriptionKey_ReturnsFalse()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "   ",
            AzureSpeechRegion = "eastus2"
        };

        // Act
        var result = _validator.IsValid(settings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithWhitespaceRegion_ReturnsFalse()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "test-key-123",
            AzureSpeechRegion = "   "
        };

        // Act
        var result = _validator.IsValid(settings);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetValidationError_WithNullSettings_ReturnsAppropriateMessage()
    {
        // Act
        var error = _validator.GetValidationError(null!);

        // Assert
        error.Should().Be("Settings cannot be null");
    }

    [Fact]
    public void GetValidationError_WithMissingSubscriptionKey_ReturnsAppropriateMessage()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "",
            AzureSpeechRegion = "eastus2"
        };

        // Act
        var error = _validator.GetValidationError(settings);

        // Assert
        error.Should().Be("Azure Speech SubscriptionKey is missing or empty");
    }

    [Fact]
    public void GetValidationError_WithMissingRegion_ReturnsAppropriateMessage()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "test-key-123",
            AzureSpeechRegion = ""
        };

        // Act
        var error = _validator.GetValidationError(settings);

        // Assert
        error.Should().Be("Azure Speech Region is missing or empty");
    }

    [Fact]
    public void GetValidationError_WithValidSettings_ReturnsEmptyString()
    {
        // Arrange
        var settings = new ApiSettings
        {
            AzureSpeechSubscriptionKey = "test-key-123",
            AzureSpeechRegion = "eastus2"
        };

        // Act
        var error = _validator.GetValidationError(settings);

        // Assert
        error.Should().BeEmpty();
    }
}
