using Azure;
using Azure.AI.OpenAI;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using PoVicTranslate.Web.Configuration;
using System.ClientModel;
using System.Diagnostics;

namespace PoVicTranslate.Web.Services;

/// <summary>
/// Service for translating modern English text to Victorian-era English using Azure OpenAI.
/// </summary>
public sealed class TranslationService : ITranslationService
{
    private readonly ChatClient _chatClient;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<TranslationService> _logger;
    private readonly string _deploymentName;

    private const string SystemPrompt = """
        You are a Victorian-era English translator. Transform modern English text into eloquent Victorian-era English,
        preserving the original meaning while using appropriate period vocabulary, formal address, and flowery prose.
        Maintain the sentiment and intent of the original text.
        Do not add any explanations, only provide the translated text.
        """;

    public TranslationService(
        IOptions<ApiSettings> apiSettings,
        TelemetryClient telemetryClient,
        ILogger<TranslationService> logger,
        ChatClient? chatClient = null)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _telemetryClient = telemetryClient;
        _logger = logger;

        var settings = apiSettings.Value;
        _deploymentName = settings.AzureOpenAIDeploymentName;

        if (chatClient != null)
        {
            _chatClient = chatClient;
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.AzureOpenAIApiKey) ||
            string.IsNullOrWhiteSpace(settings.AzureOpenAIEndpoint) ||
            string.IsNullOrWhiteSpace(settings.AzureOpenAIDeploymentName))
        {
            _logger.LogError("Azure OpenAI settings are not configured properly");
            throw new InvalidOperationException("Azure OpenAI settings are not configured.");
        }

        var openAIClient = new AzureOpenAIClient(
            new Uri(settings.AzureOpenAIEndpoint),
            new AzureKeyCredential(settings.AzureOpenAIApiKey));

        _chatClient = openAIClient.GetChatClient(settings.AzureOpenAIDeploymentName);
        _logger.LogInformation("TranslationService initialized with endpoint {Endpoint}", settings.AzureOpenAIEndpoint);
    }

    /// <inheritdoc />
    public async Task<string> TranslateToVictorianEnglishAsync(string modernText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modernText);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting translation for text length: {Length}", modernText.Length);

        var telemetry = new EventTelemetry("Translation_Request")
        {
            Properties = { ["InputLength"] = modernText.Length.ToString() }
        };

        try
        {
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage($"Please translate the following to Victorian-era English:\n\n{modernText}")
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var translatedText = response.Value.Content[0].Text;

            stopwatch.Stop();

            telemetry.Properties["Success"] = "true";
            telemetry.Properties["DurationMs"] = stopwatch.ElapsedMilliseconds.ToString();
            telemetry.Properties["OutputLength"] = translatedText.Length.ToString();
            _telemetryClient.TrackEvent(telemetry);

            _logger.LogInformation("Translation completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return translatedText;
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            stopwatch.Stop();
            telemetry.Properties["Success"] = "false";
            telemetry.Properties["ErrorType"] = "RateLimitExceeded";
            _telemetryClient.TrackEvent(telemetry);
            _logger.LogWarning(ex, "Azure OpenAI rate limit exceeded");

            return "Alas, our translation apparatus finds itself most overwhelmed at present. Pray, do make another attempt in a brief moment.";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            telemetry.Properties["Success"] = "false";
            telemetry.Properties["ErrorMessage"] = ex.Message;
            _telemetryClient.TrackEvent(telemetry);
            _logger.LogError(ex, "Translation failed: {Message}", ex.Message);

            return "Regrettably, an unforeseen circumstance has prevented the translation from being completed at this time.";
        }
    }
}
