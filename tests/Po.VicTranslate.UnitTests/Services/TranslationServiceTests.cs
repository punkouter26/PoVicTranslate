using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Po.VicTranslate.Api.Configuration;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Translation;
using Xunit;

namespace VictorianTranslator.UnitTests.Services;

public class TranslationServiceTests
{
    private readonly Mock<ILogger<TranslationService>> _mockLogger;
    private readonly Mock<ILogger<AzureOpenAIChatService>> _mockChatServiceLogger;
    private readonly Mock<IOptions<ApiSettings>> _mockOptions;
    private readonly TelemetryClient _telemetryClient;
    private readonly ApiSettings _validSettings;

    public TranslationServiceTests()
    {
        _mockLogger = new Mock<ILogger<TranslationService>>();
        _mockChatServiceLogger = new Mock<ILogger<AzureOpenAIChatService>>();
        _mockOptions = new Mock<IOptions<ApiSettings>>();

        // Create a TelemetryClient with a dummy configuration for testing
        var telemetryConfiguration = new TelemetryConfiguration();
        _telemetryClient = new TelemetryClient(telemetryConfiguration);

        _validSettings = new ApiSettings
        {
            AzureOpenAIApiKey = "test-key",
            AzureOpenAIEndpoint = "https://test.openai.azure.com/",
            AzureOpenAIDeploymentName = "gpt-4o",
            AzureSpeechSubscriptionKey = "test-speech-key",
            AzureSpeechRegion = "eastus2"
        };

        _mockOptions.Setup(x => x.Value).Returns(_validSettings);
    }

    [Fact]
    public void Constructor_WithValidSettings_ShouldInitializeSuccessfully()
    {
        // Act
        var service = new TranslationService(_mockOptions.Object, _mockLogger.Object, _mockChatServiceLogger.Object, _telemetryClient);

        // Assert
        service.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null, "https://test.com/", "gpt-4o")]
    [InlineData("", "https://test.com/", "gpt-4o")]
    [InlineData("key", null, "gpt-4o")]
    [InlineData("key", "", "gpt-4o")]
    [InlineData("key", "https://test.com/", null)]
    [InlineData("key", "https://test.com/", "")]
    public void Constructor_WithInvalidSettings_ShouldThrowInvalidOperationException(
        string? apiKey, string? endpoint, string? deploymentName)
    {
        // Arrange
        var invalidSettings = new ApiSettings
        {
            AzureOpenAIApiKey = apiKey!,
            AzureOpenAIEndpoint = endpoint!,
            AzureOpenAIDeploymentName = deploymentName!
        };
        _mockOptions.Setup(x => x.Value).Returns(invalidSettings);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new TranslationService(_mockOptions.Object, _mockLogger.Object, _mockChatServiceLogger.Object, _telemetryClient));
    }

    [Fact]
    public void Constructor_WithInvalidEndpointFormat_ShouldThrowException()
    {
        // Arrange
        var invalidSettings = new ApiSettings
        {
            AzureOpenAIApiKey = "key",
            AzureOpenAIEndpoint = "not-a-valid-url",
            AzureOpenAIDeploymentName = "gpt-4o"
        };
        _mockOptions.Setup(x => x.Value).Returns(invalidSettings);

        // Act & Assert
        Assert.Throws<UriFormatException>(() =>
            new TranslationService(_mockOptions.Object, _mockLogger.Object, _mockChatServiceLogger.Object, _telemetryClient));
    }

    [Fact]
    public void Constructor_ShouldLogSuccessfulInitialization()
    {
        // Act
        var service = new TranslationService(_mockOptions.Object, _mockLogger.Object, _mockChatServiceLogger.Object, _telemetryClient);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
