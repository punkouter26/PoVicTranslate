using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Po.VicTranslate.Api.Services;

/// <summary>
/// Phase 4: Custom telemetry service for tracking high-value business metrics
/// </summary>
public interface ICustomTelemetryService
{
    void TrackTranslationRequest(string inputLanguage, int textLength, bool success, long durationMs);
    void TrackLyricsAccess(string songTitle, string artist, string accessType);
    void TrackAudioSynthesis(int textLength, bool success, long durationMs, int? audioSizeBytes = null);
    void TrackDataUsage(string operation, string entityType, int recordCount);
    void TrackPerformanceMetric(string operationName, long durationMs, bool success);
    void TrackUserActivity(string activity, string? userId = null);
}

public class CustomTelemetryService : ICustomTelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<CustomTelemetryService> _logger;

    public CustomTelemetryService(TelemetryClient telemetryClient, ILogger<CustomTelemetryService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    /// <summary>
    /// Track translation request with language detection and performance
    /// KQL: customEvents | where name == "TranslationRequest" | summarize count() by tostring(customDimensions.Success)
    /// </summary>
    public void TrackTranslationRequest(string inputLanguage, int textLength, bool success, long durationMs)
    {
        var properties = new Dictionary<string, string>
        {
            { "InputLanguage", inputLanguage },
            { "Success", success.ToString() },
            { "TextLengthCategory", GetTextLengthCategory(textLength) }
        };

        var metrics = new Dictionary<string, double>
        {
            { "TextLength", textLength },
            { "DurationMs", durationMs }
        };

        _telemetryClient.TrackEvent("TranslationRequest", properties, metrics);

        _logger.LogInformation(
            "Translation request: Language={InputLanguage}, Length={TextLength}, Success={Success}, Duration={DurationMs}ms",
            inputLanguage, textLength, success, durationMs);
    }

    /// <summary>
    /// Track lyrics access patterns for content analytics
    /// KQL: customEvents | where name == "LyricsAccess" | summarize count() by tostring(customDimensions.SongTitle)
    /// </summary>
    public void TrackLyricsAccess(string songTitle, string artist, string accessType)
    {
        var properties = new Dictionary<string, string>
        {
            { "SongTitle", songTitle },
            { "Artist", artist },
            { "AccessType", accessType } // View, Create, Update, Delete
        };

        _telemetryClient.TrackEvent("LyricsAccess", properties);

        _logger.LogInformation(
            "Lyrics accessed: Song={SongTitle}, Artist={Artist}, Type={AccessType}",
            songTitle, artist, accessType);
    }

    /// <summary>
    /// Track audio synthesis usage and performance
    /// KQL: customEvents | where name == "AudioSynthesis" | summarize avg(customMeasurements.DurationMs), avg(customMeasurements.AudioSizeBytes)
    /// </summary>
    public void TrackAudioSynthesis(int textLength, bool success, long durationMs, int? audioSizeBytes = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Success", success.ToString() },
            { "TextLengthCategory", GetTextLengthCategory(textLength) }
        };

        var metrics = new Dictionary<string, double>
        {
            { "TextLength", textLength },
            { "DurationMs", durationMs }
        };

        if (audioSizeBytes.HasValue)
        {
            metrics["AudioSizeBytes"] = audioSizeBytes.Value;
        }

        _telemetryClient.TrackEvent("AudioSynthesis", properties, metrics);

        _logger.LogInformation(
            "Audio synthesis: Length={TextLength}, Success={Success}, Duration={DurationMs}ms, Size={AudioSize} bytes",
            textLength, success, durationMs, audioSizeBytes ?? 0);
    }

    /// <summary>
    /// Track data operations for usage analytics
    /// KQL: customEvents | where name == "DataUsage" | summarize sum(customMeasurements.RecordCount) by tostring(customDimensions.Operation)
    /// </summary>
    public void TrackDataUsage(string operation, string entityType, int recordCount)
    {
        var properties = new Dictionary<string, string>
        {
            { "Operation", operation }, // Read, Create, Update, Delete, Query
            { "EntityType", entityType } // Lyrics, Translation, Audio
        };

        var metrics = new Dictionary<string, double>
        {
            { "RecordCount", recordCount }
        };

        _telemetryClient.TrackEvent("DataUsage", properties, metrics);

        _logger.LogInformation(
            "Data usage: Operation={Operation}, Entity={EntityType}, Records={RecordCount}",
            operation, entityType, recordCount);
    }

    /// <summary>
    /// Track custom performance metrics for operations
    /// KQL: customEvents | where name == "PerformanceMetric" | summarize percentile(customMeasurements.DurationMs, 95) by tostring(customDimensions.OperationName)
    /// </summary>
    public void TrackPerformanceMetric(string operationName, long durationMs, bool success)
    {
        var properties = new Dictionary<string, string>
        {
            { "OperationName", operationName },
            { "Success", success.ToString() }
        };

        var metrics = new Dictionary<string, double>
        {
            { "DurationMs", durationMs }
        };

        _telemetryClient.TrackEvent("PerformanceMetric", properties, metrics);

        if (durationMs > 5000) // Log slow operations (>5 seconds)
        {
            _logger.LogWarning(
                "Slow operation detected: {OperationName} took {DurationMs}ms",
                operationName, durationMs);
        }
    }

    /// <summary>
    /// Track user activity for engagement analytics
    /// KQL: customEvents | where name == "UserActivity" | summarize count() by tostring(customDimensions.Activity), bin(timestamp, 1h)
    /// </summary>
    public void TrackUserActivity(string activity, string? userId = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Activity", activity }
        };

        if (!string.IsNullOrEmpty(userId))
        {
            properties["UserId"] = userId;
        }

        _telemetryClient.TrackEvent("UserActivity", properties);

        _logger.LogInformation("User activity: {Activity}, User={UserId}", activity, userId ?? "Anonymous");
    }

    private static string GetTextLengthCategory(int length)
    {
        return length switch
        {
            < 50 => "Short",
            < 200 => "Medium",
            < 1000 => "Long",
            _ => "VeryLong"
        };
    }
}
