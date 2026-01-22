using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using PoVicTranslate.Web.Models;
using PoVicTranslate.Web.Services;
using PoVicTranslate.Web.Services.Validation;

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

        _mockOpenAIValidator.Setup(v => v.Name).Returns("Azure OpenAI Configuration");
        _mockSpeechValidator.Setup(v => v.Name).Returns("Azure Speech Service Connection");
        _mockInternetValidator.Setup(v => v.Name).Returns("Internet Connectivity");

        var validators = new List<IDiagnosticValidator>
        {
            _mockOpenAIValidator.Object,
            _mockSpeechValidator.Object,
            _mockInternetValidator.Object
        };

        _validator = new ConfigurationValidator(validators, _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateAllAsync_WhenAllValidatorsPass_ReturnsTrue()
    {
        // Arrange
        _mockOpenAIValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure OpenAI Configuration", true, "Healthy", "All checks passed"));

        _mockSpeechValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure Speech Service Connection", true, "Healthy", "All checks passed"));

        _mockInternetValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Internet Connectivity", true, "Healthy", "All checks passed"));

        // Act
        var result = await _validator.ValidateAllAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAllAsync_WhenOneValidatorFails_ReturnsFalse()
    {
        // Arrange
        _mockOpenAIValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure OpenAI Configuration", true, "Healthy", "All checks passed"));

        _mockSpeechValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure Speech Service Connection", false, "Unhealthy", "Configuration missing"));

        _mockInternetValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Internet Connectivity", true, "Healthy", "All checks passed"));

        // Act
        var result = await _validator.ValidateAllAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAllAsync_WhenAllValidatorsFail_ReturnsFalse()
    {
        // Arrange
        _mockOpenAIValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure OpenAI Configuration", false, "Unhealthy", "Configuration missing"));

        _mockSpeechValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure Speech Service Connection", false, "Unhealthy", "Configuration missing"));

        _mockInternetValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Internet Connectivity", false, "Unhealthy", "No connection"));

        // Act
        var result = await _validator.ValidateAllAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAllAsync_WithEmptyValidators_ReturnsTrue()
    {
        // Arrange
        var emptyValidators = new List<IDiagnosticValidator>();
        var validator = new ConfigurationValidator(emptyValidators, _mockLogger.Object);

        // Act
        var result = await validator.ValidateAllAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAllAsync_CallsAllValidators()
    {
        // Arrange
        _mockOpenAIValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure OpenAI Configuration", true, "Healthy", null));

        _mockSpeechValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Azure Speech Service Connection", true, "Healthy", null));

        _mockInternetValidator
            .Setup(v => v.ValidateAsync())
            .ReturnsAsync(new DiagnosticResult("Internet Connectivity", true, "Healthy", null));

        // Act
        await _validator.ValidateAllAsync();

        // Assert
        _mockOpenAIValidator.Verify(v => v.ValidateAsync(), Times.Once);
        _mockSpeechValidator.Verify(v => v.ValidateAsync(), Times.Once);
        _mockInternetValidator.Verify(v => v.ValidateAsync(), Times.Once);
    }
}
