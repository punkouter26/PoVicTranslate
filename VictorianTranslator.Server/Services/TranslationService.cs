using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using VictorianTranslator.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace VictorianTranslator.Services;

public class TranslationService : ITranslationService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly string _deploymentName;
    private readonly ILogger<TranslationService> _logger;
    private readonly TelemetryClient _telemetryClient;

    public TranslationService(
        IOptions<ApiSettings> apiSettings, 
        ILogger<TranslationService> logger,
        TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
        var settings = apiSettings.Value;

        if (string.IsNullOrWhiteSpace(settings.AzureOpenAIApiKey) ||
            string.IsNullOrWhiteSpace(settings.AzureOpenAIEndpoint) ||
            string.IsNullOrWhiteSpace(settings.AzureOpenAIDeploymentName))
        {
            _logger.LogError("Azure OpenAI settings (ApiKey, Endpoint, DeploymentName) are not configured properly in appsettings.json.");
            throw new InvalidOperationException("Azure OpenAI settings are not configured.");
        }

        _deploymentName = settings.AzureOpenAIDeploymentName;

        try
        {
            // Using API Key authentication with the new AzureOpenAIClient
            _openAIClient = new AzureOpenAIClient(new Uri(settings.AzureOpenAIEndpoint), new AzureKeyCredential(settings.AzureOpenAIApiKey));
            _logger.LogInformation("AzureOpenAIClient initialized successfully with endpoint {Endpoint}", settings.AzureOpenAIEndpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AzureOpenAIClient with endpoint {Endpoint}", settings.AzureOpenAIEndpoint);
            throw; // Re-throw the exception after logging
        }
    }

    public async Task<string> TranslateToVictorianEnglishAsync(string modernText)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting translation for text: '{ModernText}'", modernText);

        // Track custom event for translation request
        var telemetry = new EventTelemetry("Translation_Request");
        telemetry.Properties["InputLength"] = modernText.Length.ToString();
        telemetry.Properties["InputText"] = modernText.Length > 100 ? modernText.Substring(0, 100) + "..." : modernText;

        // System prompt defining the role and task for the AI model
        var systemPrompt = @"You are a highly skilled translator specializing in converting modern English into authentic Victorian-era English. 
Your task is to translate the user's text while adhering strictly to the following rules:
1. Use formal, elaborate, and sophisticated language characteristic of the Victorian era.
2. Incorporate common Victorian expressions, idioms, and turns of phrase naturally.
3. Maintain a tone of utmost propriety, politeness, and decorum.
4. Employ a richer and more varied vocabulary than modern English.
5. Use appropriate honorifics (e.g., 'Sir', 'Madam', 'Miss') if the context suggests addressing someone, though often the input text won't provide this context.
6. Structure sentences in a more complex manner typical of the period.
7. Avoid modern slang, contractions (use 'do not' instead of 'don't'), and overly casual phrasing.
8. Respond ONLY with the translated Victorian English text. Do not include any preamble, explanation, apologies, or conversational filler. For example, do not say 'Here is the translation:' or 'I trust this meets your requirements.'";

        // User prompt containing the text to be translated
        var userPrompt = $"Pray, render the following modern text into the Queen's English of the Victorian age: '{modernText}'";

        try
        {
            _logger.LogInformation("Sending request to Azure OpenAI deployment '{DeploymentName}'", _deploymentName);

            var chatClient = _openAIClient.GetChatClient(_deploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 800,
                TopP = 0.95f,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };

            var response = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            if (response?.Value?.Content == null || response.Value.Content.Count == 0)
            {
                _logger.LogWarning("Received null or empty response from Azure OpenAI.");
                return "Regrettably, the translation could not be procured at this time.";
            }

            var content = response.Value.Content[0].Text;
            stopwatch.Stop();

            // Track successful translation
            telemetry.Properties["OutputLength"] = (content?.Length ?? 0).ToString();
            telemetry.Properties["DurationMs"] = stopwatch.ElapsedMilliseconds.ToString();
            telemetry.Properties["Success"] = "true";
            _telemetryClient.TrackEvent(telemetry);

            // Track performance metric
            _telemetryClient.TrackMetric("Translation_Duration_Ms", stopwatch.ElapsedMilliseconds);
            _telemetryClient.TrackMetric("Translation_InputLength", modernText.Length);
            _telemetryClient.TrackMetric("Translation_OutputLength", content?.Length ?? 0);

            _logger.LogInformation("Successfully received translation from Azure OpenAI in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return content ?? "The translation yielded naught but silence.";

        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Track failed translation
            telemetry.Properties["Success"] = "false";
            telemetry.Properties["ErrorMessage"] = ex.Message;
            telemetry.Properties["DurationMs"] = stopwatch.ElapsedMilliseconds.ToString();
            _telemetryClient.TrackEvent(telemetry);

            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                { "Operation", "TranslateToVictorianEnglish" },
                { "InputText", modernText.Length > 100 ? modernText.Substring(0, 100) + "..." : modernText }
            });

            _logger.LogError(ex, "Azure OpenAI API request failed with message: {Message}", ex.Message);
            throw new Exception($"Translation API error: Failed to communicate with Azure OpenAI. Please check logs for details.", ex);
        }
    }
}

/*
KQL QUERIES FOR APPLICATION INSIGHTS ANALYTICS:

// Query 1: Translation Request Volume and Success Rate (Last 24 hours)
customEvents
| where timestamp > ago(24h)
| where name == "Translation_Request"
| summarize 
    TotalRequests = count(),
    SuccessfulRequests = countif(customDimensions.Success == "true"),
    FailedRequests = countif(customDimensions.Success == "false"),
    AvgDurationMs = avg(todouble(customDimensions.DurationMs)),
    AvgInputLength = avg(todouble(customDimensions.InputLength)),
    AvgOutputLength = avg(todouble(customDimensions.OutputLength))
    by bin(timestamp, 1h)
| extend SuccessRate = round((SuccessfulRequests * 100.0) / TotalRequests, 2)
| project timestamp, TotalRequests, SuccessfulRequests, FailedRequests, SuccessRate, AvgDurationMs, AvgInputLength, AvgOutputLength
| order by timestamp desc

// Query 2: Translation Performance Trends (Response Time Percentiles)
customMetrics
| where timestamp > ago(7d)
| where name == "Translation_Duration_Ms"
| summarize 
    p50 = percentile(value, 50),
    p75 = percentile(value, 75),
    p95 = percentile(value, 95),
    p99 = percentile(value, 99),
    avg = avg(value),
    max = max(value)
    by bin(timestamp, 1h)
| project timestamp, p50, p75, p95, p99, avg, max
| order by timestamp desc

// Query 3: Error Analysis and Client Log Insights
union 
(customEvents | where name == "Translation_Request" and customDimensions.Success == "false"),
(customEvents | where name == "ClientLog" and customDimensions.Level in ("error", "warning")),
(exceptions)
| where timestamp > ago(24h)
| project 
    timestamp,
    EventType = case(
        itemType == "customEvent" and name == "Translation_Request", "Translation Error",
        itemType == "customEvent" and name == "ClientLog", "Client Error/Warning",
        itemType == "exception", "Server Exception",
        "Other"
    ),
    Message = case(
        itemType == "customEvent", tostring(customDimensions.ErrorMessage),
        itemType == "exception", outerMessage,
        ""
    ),
    Details = case(
        itemType == "customEvent" and name == "ClientLog", customDimensions,
        itemType == "exception", customDimensions,
        bag_pack("none", "none")
    )
| order by timestamp desc
| take 100
*/
