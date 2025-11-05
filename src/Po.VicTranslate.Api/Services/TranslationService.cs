using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Po.VicTranslate.Api.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;
using System.ClientModel;
using Po.VicTranslate.Api.Services.Translation;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Service for translating modern English text to Victorian-era English.
/// Refactored using Template Method pattern for improved maintainability (complexity reduced from 20 to ~8).
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly AzureOpenAIChatService _chatService;
    private readonly VictorianPromptBuilder _promptBuilder;
    private readonly TranslationTelemetryTracker _telemetryTracker;
    private readonly ILogger<TranslationService> _logger;

    public TranslationService(
        IOptions<ApiSettings> apiSettings,
        ILogger<TranslationService> logger,
        ILogger<AzureOpenAIChatService> chatServiceLogger,
        TelemetryClient telemetryClient)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _logger = logger;
        
        var settings = apiSettings.Value;

        if (string.IsNullOrWhiteSpace(settings.AzureOpenAIApiKey) ||
            string.IsNullOrWhiteSpace(settings.AzureOpenAIEndpoint) ||
            string.IsNullOrWhiteSpace(settings.AzureOpenAIDeploymentName))
        {
            _logger.LogError("Azure OpenAI settings (ApiKey, Endpoint, DeploymentName) are not configured properly in appsettings.json.");
            throw new InvalidOperationException("Azure OpenAI settings are not configured.");
        }

        try
        {
            // Initialize Azure OpenAI client
            var openAIClient = new AzureOpenAIClient(
                new Uri(settings.AzureOpenAIEndpoint), 
                new AzureKeyCredential(settings.AzureOpenAIApiKey));
            
            _logger.LogInformation("AzureOpenAIClient initialized successfully with endpoint {Endpoint}", settings.AzureOpenAIEndpoint);

            // Initialize extracted service classes (Template Method pattern)
            _chatService = new AzureOpenAIChatService(openAIClient, settings.AzureOpenAIDeploymentName, chatServiceLogger);
            _promptBuilder = new VictorianPromptBuilder();
            _telemetryTracker = new TranslationTelemetryTracker(telemetryClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AzureOpenAIClient with endpoint {Endpoint}", settings.AzureOpenAIEndpoint);
            throw;
        }
    }

    /// <summary>
    /// Translates modern English text to Victorian-era English using Azure OpenAI.
    /// Refactored using Template Method pattern - complexity reduced from 20 to ~8.
    /// </summary>
    public async Task<string> TranslateToVictorianEnglishAsync(string modernText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modernText);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting translation for text: '{ModernText}'", modernText);

        // Create telemetry event
        var telemetry = _telemetryTracker.CreateRequestTelemetry(modernText);

        try
        {
            // Build prompts using extracted builder
            var (systemPrompt, userPrompt) = _promptBuilder.BuildPrompts(modernText);

            // Call Azure OpenAI using extracted service
            var translatedText = await _chatService.CompleteChatAsync(systemPrompt, userPrompt);

            stopwatch.Stop();

            // Track success
            _telemetryTracker.TrackSuccess(telemetry, translatedText, stopwatch.ElapsedMilliseconds, modernText.Length);
            _logger.LogInformation("Successfully received translation from Azure OpenAI in {Duration}ms", stopwatch.ElapsedMilliseconds);

            return translatedText;
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            stopwatch.Stop();
            _telemetryTracker.TrackFailure(telemetry, ex, stopwatch.ElapsedMilliseconds, modernText, "RateLimitExceeded");
            _logger.LogWarning(ex, "Azure OpenAI rate limit exceeded. Status: {Status}", ex.Status);

            return "Alas, our translation apparatus finds itself most overwhelmed at present. Pray, do make another attempt in a brief moment.";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetryTracker.TrackFailure(telemetry, ex, stopwatch.ElapsedMilliseconds, modernText);
            _logger.LogError(ex, "Azure OpenAI API request failed with message: {Message}", ex.Message);

            return "Regrettably, an unforeseen circumstance has prevented the translation from being completed at this time. Most sincere apologies for this inconvenience.";
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
