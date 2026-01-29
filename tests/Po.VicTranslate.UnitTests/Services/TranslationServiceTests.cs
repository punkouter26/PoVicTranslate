using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using PoVicTranslate.Web.Configuration;
using PoVicTranslate.Web.Services;
using System.ClientModel;
using System.ClientModel.Primitives;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class TranslationServiceTests
{
    private readonly Mock<ILogger<TranslationService>> _mockLogger;
    private readonly Mock<IOptions<ApiSettings>> _mockOptions;
    private readonly TelemetryClient _telemetryClient;
    private readonly ApiSettings _validSettings;

    public TranslationServiceTests()
    {
        _mockLogger = new Mock<ILogger<TranslationService>>();
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
        var service = new TranslationService(_mockOptions.Object, _telemetryClient, _mockLogger.Object);

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
            new TranslationService(_mockOptions.Object, _telemetryClient, _mockLogger.Object));
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
            new TranslationService(_mockOptions.Object, _telemetryClient, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_ShouldLogSuccessfulInitialization()
    {
        // Act
        var service = new TranslationService(_mockOptions.Object, _telemetryClient, _mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TranslateToVictorianEnglishAsync_WithValidText_ShouldReturnTranslation()
    {
        // Arrange
        var mockChatClient = new Mock<ChatClient>("gpt-4o", new ApiKeyCredential("key"), new OpenAIClientOptions());
        var expectedTranslation = "Good morrow.";

        var content = new ChatMessageContent(expectedTranslation);
        var chatCompletion = OpenAIChatModelFactory.ChatCompletion(content: content);

        var mockResponse = new Mock<PipelineResponse>();
        mockResponse.Setup(r => r.Status).Returns(200);

        var clientResult = ClientResult.FromValue(chatCompletion, mockResponse.Object);

        mockChatClient.Setup(x => x.CompleteChatAsync(
            It.IsAny<ChatMessage[]>()))
            .ReturnsAsync(clientResult);

        var service = new TranslationService(
            _mockOptions.Object,
            _telemetryClient,
            _mockLogger.Object,
            mockChatClient.Object);

        // Act
        var result = await service.TranslateToVictorianEnglishAsync("Hello");

        // Assert
        result.Should().Be(expectedTranslation);
    }

    [Fact]
    public async Task TranslateToVictorianEnglishAsync_WhenRateLimited_ShouldReturnPoliteError()
    {
        // Arrange
        var mockChatClient = new Mock<ChatClient>("gpt-4o", new ApiKeyCredential("key"), new OpenAIClientOptions());

        var mockResponse = new Mock<PipelineResponse>();
        mockResponse.Setup(r => r.Status).Returns(429);

        var exception = new ClientResultException(mockResponse.Object);

        mockChatClient.Setup(x => x.CompleteChatAsync(
            It.IsAny<ChatMessage[]>()))
            .ThrowsAsync(exception);

        var service = new TranslationService(
            _mockOptions.Object,
            _telemetryClient,
            _mockLogger.Object,
            mockChatClient.Object);

        // Act
        var result = await service.TranslateToVictorianEnglishAsync("Hello");

        // Assert
        result.Should().Contain("Alas, our translation apparatus finds itself most overwhelmed");
    }

    [Fact]
    public async Task TranslateToVictorianEnglishAsync_WhenErrorOccurs_ShouldReturnErrorMessage()
    {
        // Arrange
        var mockChatClient = new Mock<ChatClient>("gpt-4o", new ApiKeyCredential("key"), new OpenAIClientOptions());

        mockChatClient.Setup(x => x.CompleteChatAsync(
            It.IsAny<ChatMessage[]>()))
            .ThrowsAsync(new InvalidOperationException("Generic error"));

        var service = new TranslationService(
            _mockOptions.Object,
            _telemetryClient,
            _mockLogger.Object,
            mockChatClient.Object);

        // Act
        var result = await service.TranslateToVictorianEnglishAsync("Hello");

        // Assert
        result.Should().Contain("Regrettably, an unforeseen circumstance");
    }
}
