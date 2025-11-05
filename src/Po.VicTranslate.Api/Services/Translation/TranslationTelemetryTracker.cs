using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Po.VicTranslate.Api.Services.Translation;

/// <summary>
/// Handles telemetry tracking for translation operations.
/// Extracted from TranslationService to reduce complexity and improve testability.
/// </summary>
public class TranslationTelemetryTracker
{
    private readonly TelemetryClient _telemetryClient;

    public TranslationTelemetryTracker(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    /// <summary>
    /// Creates an initial telemetry event for a translation request.
    /// </summary>
    public EventTelemetry CreateRequestTelemetry(string inputText)
    {
        var telemetry = new EventTelemetry("Translation_Request");
        telemetry.Properties["InputLength"] = inputText.Length.ToString();
        telemetry.Properties["InputText"] = inputText.Length > 100 
            ? inputText.Substring(0, 100) + "..." 
            : inputText;
        
        return telemetry;
    }

    /// <summary>
    /// Tracks a successful translation with performance metrics.
    /// </summary>
    public void TrackSuccess(EventTelemetry telemetry, string outputText, long durationMs, int inputLength)
    {
        // Update telemetry with success data
        telemetry.Properties["OutputLength"] = outputText.Length.ToString();
        telemetry.Properties["DurationMs"] = durationMs.ToString();
        telemetry.Properties["Success"] = "true";
        _telemetryClient.TrackEvent(telemetry);

        // Track performance metrics
        _telemetryClient.TrackMetric("Translation_Duration_Ms", durationMs);
        _telemetryClient.TrackMetric("Translation_InputLength", inputLength);
        _telemetryClient.TrackMetric("Translation_OutputLength", outputText.Length);
    }

    /// <summary>
    /// Tracks a failed translation with error details.
    /// </summary>
    public void TrackFailure(EventTelemetry telemetry, Exception exception, long durationMs, string inputText, string errorType = "General")
    {
        // Update telemetry with failure data
        telemetry.Properties["Success"] = "false";
        telemetry.Properties["ErrorType"] = errorType;
        telemetry.Properties["ErrorMessage"] = exception.Message;
        telemetry.Properties["DurationMs"] = durationMs.ToString();
        _telemetryClient.TrackEvent(telemetry);

        // Track exception with additional context
        _telemetryClient.TrackException(exception, new Dictionary<string, string>
        {
            { "Operation", "TranslateToVictorianEnglish" },
            { "ErrorType", errorType },
            { "InputText", inputText.Length > 100 ? inputText.Substring(0, 100) + "..." : inputText }
        });
    }
}
