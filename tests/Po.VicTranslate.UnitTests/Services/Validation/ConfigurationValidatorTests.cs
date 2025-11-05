using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Validation;

namespace Po.VicTranslate.UnitTests.Services.Validation;

public class ConfigurationValidatorTests
{
    private readonly Mock<ILogger<ConfigurationValidator>> _mockLogger;
    private readonly Mock<IDiagnosticValidator> _mockOpenAIValidator;
    private readonly Mock<IDiagnosticValidator> _mockSpeechValidator;
    private readonly Mock<IDiagnosticValidator> _mockInternetValidator;
    private readonly ConfigurationValidator _validator;

    public ConfigurationValidatorTests()
    {
        _mockLogger = new Mock<ILogger<ConfigurationValidator>>();
        _mockOpenAIValidator = new Mock<IDiagnosticValidator>();
        _mockSpeechValidator = new Mock<IDiagnosticValidator>();
        _mockInternetValidator = new Mock<IDiagnosticValidator>();

        _mockOpenAIValidator.Setup(v => v.CheckName).Returns("Azure OpenAI Configuration");
        _mockSpeechValidator.Setup(v => v.CheckName).Returns("Azure Speech Service Connection");
        _mockInternetValidator.Setup(v => v.CheckName).Returns("Internet Connectivity");

        var validators = new List<IDiagnosticValidator>
        {
            _mockOpenAIValidator.Object,
            _mockSpeechValidator.Object,
            _mockInternetValidator.Object
        };

        _validator = new ConfigurationValidator(validators, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullValidators_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConfigurationValidator(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAzureOpenAI_DelegatesToCorrectValidator()
    {
        // Arrange
        var expectedResult = new DiagnosticResult
        {
            CheckName = "Azure OpenAI Configuration",
            Success = true,
            Message = "OpenAI config is valid"
        };

        _mockOpenAIValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var result = _validator.ValidateAzureOpenAI();

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _mockOpenAIValidator.Verify(v => v.ValidateAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateAzureSpeechAsync_DelegatesToCorrectValidator()
    {
        // Arrange
        var expectedResult = new DiagnosticResult
        {
            CheckName = "Azure Speech Service Connection",
            Success = true,
            Message = "Speech service connected"
        };

        _mockSpeechValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _validator.ValidateAzureSpeechAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _mockSpeechValidator.Verify(v => v.ValidateAsync(), Times.Once);
    }

    [Fact]
    public void ValidateInternetConnectivity_DelegatesToCorrectValidator()
    {
        // Arrange
        var expectedResult = new DiagnosticResult
        {
            CheckName = "Internet Connectivity",
            Success = true,
            Message = "Internet is available"
        };

        _mockInternetValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(expectedResult);

        // Act
        var result = _validator.ValidateInternetConnectivity();

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _mockInternetValidator.Verify(v => v.ValidateAsync(), Times.Once);
    }

    [Fact]
    public void ValidateAzureOpenAI_WhenValidatorNotRegistered_ReturnsFailureResult()
    {
        // Arrange
        var emptyValidators = new List<IDiagnosticValidator>();
        var validator = new ConfigurationValidator(emptyValidators, _mockLogger.Object);

        // Act
        var result = validator.ValidateAzureOpenAI();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.CheckName.Should().Be("Azure OpenAI Configuration");
        result.Message.Should().Be("Validator not registered");
    }

    [Fact]
    public async Task ValidateAzureSpeechAsync_WhenValidatorNotRegistered_ReturnsFailureResult()
    {
        // Arrange
        var emptyValidators = new List<IDiagnosticValidator>();
        var validator = new ConfigurationValidator(emptyValidators, _mockLogger.Object);

        // Act
        var result = await validator.ValidateAzureSpeechAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.CheckName.Should().Be("Azure Speech Service Connection");
        result.Message.Should().Be("Validator not registered");
    }

    [Fact]
    public void ValidateInternetConnectivity_WhenValidatorNotRegistered_ReturnsFailureResult()
    {
        // Arrange
        var emptyValidators = new List<IDiagnosticValidator>();
        var validator = new ConfigurationValidator(emptyValidators, _mockLogger.Object);

        // Act
        var result = validator.ValidateInternetConnectivity();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.CheckName.Should().Be("Internet Connectivity");
        result.Message.Should().Be("Validator not registered");
    }
}
